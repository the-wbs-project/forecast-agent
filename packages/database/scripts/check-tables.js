#!/usr/bin/env node

const { execSync } = require('child_process');
const path = require('path');
const fs = require('fs');

// Load environment variables
const dotenvPath = path.join(__dirname, '..', '.env');
if (fs.existsSync(dotenvPath)) {
  require('dotenv').config({ path: dotenvPath });
}

const config = {
  server: process.env.DB_SERVER || 'localhost',
  database: process.env.DB_DATABASE || 'WeatherGuardDB',
  user: process.env.DB_USER || 'sa',
  password: process.env.DB_PASSWORD || 'YourStrongPassword123!',
};

const query = `
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE' 
ORDER BY TABLE_NAME;
`;

const command = [
  'sqlcmd',
  `-S ${config.server}`,
  `-d ${config.database}`,
  `-U ${config.user}`,
  `-P ${config.password}`,
  `-Q "${query}"`,
  '-h -1',
  '-W',
  '-s ","',
  '-C'
].join(' ');

try {
  console.log('Checking existing tables in database...\n');
  const output = execSync(command, { encoding: 'utf8' });
  const tables = output.trim().split('\n').filter(t => t && !t.includes('rows affected'));
  
  console.log('Existing tables:');
  tables.forEach(table => console.log(`  - ${table.trim()}`));
  console.log(`\nTotal: ${tables.length} tables`);
} catch (error) {
  console.error('Error:', error.message);
}