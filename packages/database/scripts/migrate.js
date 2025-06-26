#!/usr/bin/env node

const fs = require('fs');
const path = require('path');
const { execSync } = require('child_process');

// Load environment variables from .env file
const dotenvPath = path.join(__dirname, '..', '.env');

if (fs.existsSync(dotenvPath)) {
  require('dotenv').config({ path: dotenvPath });
} else {
  // Try loading from parent directories
  const rootEnvPath = path.join(__dirname, '..', '.env');
  if (fs.existsSync(rootEnvPath)) {
    require('dotenv').config({ path: rootEnvPath });
  }
}

// Configuration
const config = {
  server: process.env.DB_SERVER || 'localhost',
  database: process.env.DB_DATABASE || 'WeatherGuardDB',
  user: process.env.DB_USER || 'sa',
  password: process.env.DB_PASSWORD || 'YourStrongPassword123!',
  trustServerCertificate: process.env.DB_TRUST_CERT !== 'false',
};

// Migrations directory
const migrationsDir = path.join(__dirname, '..', 'schema');
const appliedMigrationsFile = path.join(__dirname, '..', '.applied-migrations.json');

// Load applied migrations history
function loadAppliedMigrations() {
  try {
    if (fs.existsSync(appliedMigrationsFile)) {
      return JSON.parse(fs.readFileSync(appliedMigrationsFile, 'utf8'));
    }
  } catch (error) {
    console.error('Error loading migration history:', error);
  }
  return [];
}

// Save applied migrations
function saveAppliedMigrations(migrations) {
  fs.writeFileSync(appliedMigrationsFile, JSON.stringify(migrations, null, 2));
}

// Execute SQL file using sqlcmd
function executeSqlFile(filePath, description) {
  console.log(`\nExecuting: ${description}`);
  
  try {
    // Build sqlcmd command
    const command = [
      'sqlcmd',
      `-S ${config.server}`,
      `-d ${config.database}`, // Explicitly specify database
      `-U ${config.user}`,
      `-P ${config.password}`,
      `-i "${filePath}"`,
      '-b', // On error batch abort
      '-m 1', // Error messages level
    ];

    if (config.trustServerCertificate) {
      command.push('-C'); // Trust server certificate
    }

    // Execute the command
    const output = execSync(command.join(' '), {
      encoding: 'utf8',
      stdio: 'pipe',
    });

    console.log('✓ Success');
    if (output && output.trim()) {
      console.log('Output:', output);
    }
    return true;
  } catch (error) {
    console.error('✗ Failed');
    console.error('Error:', error.message);
    if (error.stdout) {
      console.error('stdout:', error.stdout.toString());
    }
    if (error.stderr) {
      console.error('stderr:', error.stderr.toString());
    }
    return false;
  }
}

// Get migration files
function getMigrationFiles() {
  const files = fs.readdirSync(migrationsDir)
    .filter(file => file.endsWith('.sql'))
    .sort();
  
  return files.map(file => ({
    filename: file,
    path: path.join(migrationsDir, file),
  }));
}

// Run migrations
async function runMigrations() {
  console.log('WeatherGuard Database Migration Tool');
  console.log('===================================');
  console.log(`Server: ${config.server}`);
  console.log(`Database: ${config.database}`);
  console.log(`User: ${config.user}`);
  console.log(`Trust Certificate: ${config.trustServerCertificate}`);
  
  // Debug: Show which .env file was loaded
  if (fs.existsSync(path.join(__dirname, '..', '.env'))) {
    console.log(`Using .env from: ${path.join(__dirname, '..', '.env')}`);
  } else if (fs.existsSync(path.join(__dirname, '..', '..', '..', '.env'))) {
    console.log(`Using .env from: ${path.join(__dirname, '..', '..', '..', '.env')}`);
  } else {
    console.log('No .env file found, using default values');
  }
  
  // Check if sqlcmd is available
  try {
    execSync('sqlcmd -?', { stdio: 'ignore' });
  } catch (error) {
    console.error('\nError: sqlcmd is not installed or not in PATH');
    console.error('Please install SQL Server command line tools:');
    console.error('- Windows: Install SQL Server Management Studio or SQL Server Command Line Utilities');
    console.error('- macOS: brew install sqlcmd');
    console.error('- Linux: Follow Microsoft documentation for installing mssql-tools');
    process.exit(1);
  }

  // Get applied migrations
  const appliedMigrations = loadAppliedMigrations();
  const appliedSet = new Set(appliedMigrations.map(m => m.filename));

  // Get all migration files
  const migrationFiles = getMigrationFiles();
  
  // Find pending migrations
  const pendingMigrations = migrationFiles.filter(m => !appliedSet.has(m.filename));

  if (pendingMigrations.length === 0) {
    console.log('\n✓ All migrations are up to date');
    return;
  }

  console.log(`\nFound ${pendingMigrations.length} pending migration(s):`);
  pendingMigrations.forEach(m => console.log(`  - ${m.filename}`));

  // Execute pending migrations
  const results = [];
  for (const migration of pendingMigrations) {
    const success = executeSqlFile(migration.path, migration.filename);
    
    if (success) {
      const migrationRecord = {
        filename: migration.filename,
        appliedAt: new Date().toISOString(),
      };
      appliedMigrations.push(migrationRecord);
      results.push({ ...migrationRecord, success: true });
    } else {
      results.push({
        filename: migration.filename,
        success: false,
        error: 'Migration failed',
      });
      
      // Stop on first failure
      console.error('\n✗ Migration failed. Stopping execution.');
      break;
    }
  }

  // Save updated migration history
  saveAppliedMigrations(appliedMigrations);

  // Summary
  console.log('\nMigration Summary:');
  console.log('==================');
  const successful = results.filter(r => r.success).length;
  const failed = results.filter(r => !r.success).length;
  
  console.log(`✓ Successful: ${successful}`);
  if (failed > 0) {
    console.log(`✗ Failed: ${failed}`);
  }

  // Exit with error code if any migrations failed
  if (failed > 0) {
    process.exit(1);
  }
}

// Handle stored procedures separately
async function runStoredProcedures() {
  const spDir = path.join(__dirname, '..', 'stored-procedures');
  
  if (!fs.existsSync(spDir)) {
    return;
  }

  const spFiles = fs.readdirSync(spDir)
    .filter(file => file.endsWith('.sql'))
    .sort();

  if (spFiles.length === 0) {
    return;
  }

  console.log('\nApplying Stored Procedures:');
  console.log('==========================');

  for (const file of spFiles) {
    const filePath = path.join(spDir, file);
    executeSqlFile(filePath, `Stored Procedure: ${file}`);
  }
}

// Main execution
async function main() {
  try {
    // Run schema migrations
    await runMigrations();
    
    // Run stored procedures
    await runStoredProcedures();
    
    console.log('\n✓ Database migration completed successfully');
  } catch (error) {
    console.error('\n✗ Migration failed:', error.message);
    process.exit(1);
  }
}

// Run if called directly
if (require.main === module) {
  main();
}

module.exports = { runMigrations, executeSqlFile };