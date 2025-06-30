import { KVService } from './kv.service';
import { UserInfo, TokenPayload } from '../dto';

export interface AuthDataService {
  getUserById(userId: string): Promise<UserInfo | null>;
  getUserByEmail(email: string): Promise<UserInfo | null>;
  createUser(user: UserInfo): Promise<void>;
  updateUser(userId: string, updates: Partial<UserInfo>): Promise<void>;
  deleteUser(userId: string): Promise<void>;
  storeRefreshToken(userId: string, refreshToken: string, expiresAt: Date): Promise<void>;
  getRefreshToken(refreshToken: string): Promise<{ userId: string; expiresAt: Date } | null>;
  revokeRefreshToken(refreshToken: string): Promise<void>;
  storePasswordResetToken(email: string, token: string, expiresAt: Date): Promise<void>;
  getPasswordResetToken(token: string): Promise<{ email: string; expiresAt: Date } | null>;
  revokePasswordResetToken(token: string): Promise<void>;
}

export class KVAuthDataService implements AuthDataService {
  constructor(private kvService: KVService) {}

  private getUserKey(userId: string): string {
    return `user:${userId}`;
  }

  private getEmailKey(email: string): string {
    return `email:${email.toLowerCase()}`;
  }

  private getRefreshTokenKey(refreshToken: string): string {
    return `refresh_token:${refreshToken}`;
  }

  private getPasswordResetTokenKey(token: string): string {
    return `password_reset:${token}`;
  }

  async getUserById(userId: string): Promise<UserInfo | null> {
    return await this.kvService.get<UserInfo>(this.getUserKey(userId));
  }

  async getUserByEmail(email: string): Promise<UserInfo | null> {
    const userId = await this.kvService.get<string>(this.getEmailKey(email));
    if (!userId) return null;
    return await this.getUserById(userId);
  }

  async createUser(user: UserInfo): Promise<void> {
    const userKey = this.getUserKey(user.id);
    const emailKey = this.getEmailKey(user.email);
    
    const metadata = {
      id: user.id,
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
      tags: ['user', user.role]
    };

    await Promise.all([
      this.kvService.put(userKey, user, metadata),
      this.kvService.put(emailKey, user.id, metadata)
    ]);
  }

  async updateUser(userId: string, updates: Partial<UserInfo>): Promise<void> {
    const existingUser = await this.getUserById(userId);
    if (!existingUser) throw new Error('User not found');

    const updatedUser = { ...existingUser, ...updates };
    const metadata = {
      id: userId,
      createdAt: existingUser.id, // Keep original created date
      updatedAt: new Date().toISOString(),
      tags: ['user', updatedUser.role]
    };

    await this.kvService.put(this.getUserKey(userId), updatedUser, metadata);

    // Update email mapping if email changed
    if (updates.email && updates.email !== existingUser.email) {
      await this.kvService.delete(this.getEmailKey(existingUser.email));
      await this.kvService.put(this.getEmailKey(updates.email), userId, metadata);
    }
  }

  async deleteUser(userId: string): Promise<void> {
    const user = await this.getUserById(userId);
    if (!user) return;

    await Promise.all([
      this.kvService.delete(this.getUserKey(userId)),
      this.kvService.delete(this.getEmailKey(user.email))
    ]);
  }

  async storeRefreshToken(userId: string, refreshToken: string, expiresAt: Date): Promise<void> {
    const tokenData = { userId, expiresAt: expiresAt.toISOString() };
    const metadata = {
      id: refreshToken,
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
      createdBy: userId,
      tags: ['refresh_token']
    };

    await this.kvService.put(this.getRefreshTokenKey(refreshToken), tokenData, metadata);
  }

  async getRefreshToken(refreshToken: string): Promise<{ userId: string; expiresAt: Date } | null> {
    const tokenData = await this.kvService.get<{ userId: string; expiresAt: string }>(
      this.getRefreshTokenKey(refreshToken)
    );
    
    if (!tokenData) return null;
    
    return {
      userId: tokenData.userId,
      expiresAt: new Date(tokenData.expiresAt)
    };
  }

  async revokeRefreshToken(refreshToken: string): Promise<void> {
    await this.kvService.delete(this.getRefreshTokenKey(refreshToken));
  }

  async storePasswordResetToken(email: string, token: string, expiresAt: Date): Promise<void> {
    const tokenData = { email: email.toLowerCase(), expiresAt: expiresAt.toISOString() };
    const metadata = {
      id: token,
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
      tags: ['password_reset']
    };

    await this.kvService.put(this.getPasswordResetTokenKey(token), tokenData, metadata);
  }

  async getPasswordResetToken(token: string): Promise<{ email: string; expiresAt: Date } | null> {
    const tokenData = await this.kvService.get<{ email: string; expiresAt: string }>(
      this.getPasswordResetTokenKey(token)
    );
    
    if (!tokenData) return null;
    
    return {
      email: tokenData.email,
      expiresAt: new Date(tokenData.expiresAt)
    };
  }

  async revokePasswordResetToken(token: string): Promise<void> {
    await this.kvService.delete(this.getPasswordResetTokenKey(token));
  }
}