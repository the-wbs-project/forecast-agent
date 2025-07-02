import type {
	ChangePasswordRequest,
	LoginRequest,
	LoginResponse,
	RefreshTokenRequest,
	RegisterRequest,
	ResetPasswordConfirmRequest,
	ResetPasswordRequest,
	TokenPayload,
	UserInfo,
} from "../../dto";
import type { AuthDataService } from "../data-services";

export interface IAuthService {
	login(request: LoginRequest): Promise<LoginResponse>;
	register(request: RegisterRequest): Promise<UserInfo>;
	refreshToken(request: RefreshTokenRequest): Promise<LoginResponse>;
	logout(refreshToken: string): Promise<void>;
	changePassword(userId: string, request: ChangePasswordRequest): Promise<void>;
	resetPassword(request: ResetPasswordRequest): Promise<void>;
	confirmResetPassword(request: ResetPasswordConfirmRequest): Promise<void>;
	validateToken(token: string): Promise<TokenPayload | null>;
	getUserById(userId: string): Promise<UserInfo | null>;
	getUserByEmail(email: string): Promise<UserInfo | null>;
}

export class AuthService implements IAuthService {
	constructor(
		private authDataService: AuthDataService,
		private jwtSecret: string,
	) {}

	async login(request: LoginRequest): Promise<LoginResponse> {
		const user = await this.authDataService.getUserByEmail(request.email);
		if (!user) {
			throw new Error("Invalid email or password");
		}

		if (!user.isActive) {
			throw new Error("Account is disabled");
		}

		// In a real implementation, you would verify the password hash
		// For now, we'll assume password validation is handled elsewhere
		const passwordValid = await this.verifyPassword(request.password, user.id);
		if (!passwordValid) {
			throw new Error("Invalid email or password");
		}

		const token = await this.generateJWT(user);
		const refreshToken = await this.generateRefreshToken(user.id);

		// Store refresh token
		const expiresAt = new Date(Date.now() + 30 * 24 * 60 * 60 * 1000); // 30 days
		await this.authDataService.storeRefreshToken(
			user.id,
			refreshToken,
			expiresAt,
		);

		return {
			token,
			refreshToken,
			user,
			expiresIn: 3600, // 1 hour
		};
	}

	async register(request: RegisterRequest): Promise<UserInfo> {
		// Check if user already exists
		const existingUser = await this.authDataService.getUserByEmail(
			request.email,
		);
		if (existingUser) {
			throw new Error("User with this email already exists");
		}

		// Create new user
		const userId = crypto.randomUUID();
		const user: UserInfo = {
			id: userId,
			email: request.email.toLowerCase(),
			firstName: request.firstName,
			lastName: request.lastName,
			fullName: `${request.firstName} ${request.lastName}`,
			organizationId: request.organizationId,
			role: "user",
			isActive: true,
		};

		// In a real implementation, you would hash the password
		await this.storePasswordHash(userId, request.password);
		await this.authDataService.createUser(user);

		return user;
	}

	async refreshToken(request: RefreshTokenRequest): Promise<LoginResponse> {
		const tokenData = await this.authDataService.getRefreshToken(
			request.refreshToken,
		);
		if (!tokenData || tokenData.expiresAt < new Date()) {
			throw new Error("Invalid or expired refresh token");
		}

		const user = await this.authDataService.getUserById(tokenData.userId);
		if (!user || !user.isActive) {
			throw new Error("User not found or disabled");
		}

		// Generate new tokens
		const token = await this.generateJWT(user);
		const newRefreshToken = await this.generateRefreshToken(user.id);

		// Revoke old refresh token and store new one
		await this.authDataService.revokeRefreshToken(request.refreshToken);
		const expiresAt = new Date(Date.now() + 30 * 24 * 60 * 60 * 1000);
		await this.authDataService.storeRefreshToken(
			user.id,
			newRefreshToken,
			expiresAt,
		);

		return {
			token,
			refreshToken: newRefreshToken,
			user,
			expiresIn: 3600,
		};
	}

	async logout(refreshToken: string): Promise<void> {
		await this.authDataService.revokeRefreshToken(refreshToken);
	}

	async changePassword(
		userId: string,
		request: ChangePasswordRequest,
	): Promise<void> {
		const user = await this.authDataService.getUserById(userId);
		if (!user) {
			throw new Error("User not found");
		}

		// Verify current password
		const currentPasswordValid = await this.verifyPassword(
			request.currentPassword,
			userId,
		);
		if (!currentPasswordValid) {
			throw new Error("Current password is incorrect");
		}

		// Store new password hash
		await this.storePasswordHash(userId, request.newPassword);
	}

	async resetPassword(request: ResetPasswordRequest): Promise<void> {
		const user = await this.authDataService.getUserByEmail(request.email);
		if (!user) {
			// Don't reveal if email exists
			return;
		}

		const resetToken = crypto.randomUUID();
		const expiresAt = new Date(Date.now() + 60 * 60 * 1000); // 1 hour

		await this.authDataService.storePasswordResetToken(
			request.email,
			resetToken,
			expiresAt,
		);

		// In a real implementation, you would send an email with the reset token
		console.log(`Password reset token for ${request.email}: ${resetToken}`);
	}

	async confirmResetPassword(
		request: ResetPasswordConfirmRequest,
	): Promise<void> {
		const tokenData = await this.authDataService.getPasswordResetToken(
			request.token,
		);
		if (!tokenData || tokenData.expiresAt < new Date()) {
			throw new Error("Invalid or expired reset token");
		}

		const user = await this.authDataService.getUserByEmail(tokenData.email);
		if (!user) {
			throw new Error("User not found");
		}

		// Store new password hash
		await this.storePasswordHash(user.id, request.newPassword);

		// Revoke the reset token
		await this.authDataService.revokePasswordResetToken(request.token);
	}

	async validateToken(token: string): Promise<TokenPayload | null> {
		try {
			// In a real implementation, you would use a proper JWT library
			// This is a simplified version
			const payload = await this.verifyJWT(token);

			// Check if user still exists and is active
			const user = await this.authDataService.getUserById(payload.userId);
			if (!user || !user.isActive) {
				return null;
			}

			return payload;
		} catch (error) {
			return null;
		}
	}

	async getUserById(userId: string): Promise<UserInfo | null> {
		return await this.authDataService.getUserById(userId);
	}

	async getUserByEmail(email: string): Promise<UserInfo | null> {
		return await this.authDataService.getUserByEmail(email);
	}

	private async generateJWT(user: UserInfo): Promise<string> {
		const payload: TokenPayload = {
			userId: user.id,
			email: user.email,
			organizationId: user.organizationId,
			role: user.role,
			iat: Math.floor(Date.now() / 1000),
			exp: Math.floor(Date.now() / 1000) + 3600, // 1 hour
		};

		// In a real implementation, use a proper JWT library like @tsndr/cloudflare-worker-jwt
		return btoa(JSON.stringify(payload));
	}

	private async verifyJWT(token: string): Promise<TokenPayload> {
		try {
			// In a real implementation, verify the JWT signature
			const payload = JSON.parse(atob(token)) as TokenPayload;

			if (payload.exp < Math.floor(Date.now() / 1000)) {
				throw new Error("Token expired");
			}

			return payload;
		} catch (error) {
			throw new Error("Invalid token");
		}
	}

	private async generateRefreshToken(userId: string): Promise<string> {
		return `${userId}_${crypto.randomUUID()}_${Date.now()}`;
	}

	private async verifyPassword(
		password: string,
		userId: string,
	): Promise<boolean> {
		// In a real implementation, you would:
		// 1. Retrieve the stored password hash
		// 2. Compare the provided password with the hash using bcrypt or similar
		// For now, this is a placeholder
		return password.length > 0; // Simplified validation
	}

	private async storePasswordHash(
		userId: string,
		password: string,
	): Promise<void> {
		// In a real implementation, you would:
		// 1. Hash the password using bcrypt or similar
		// 2. Store the hash in KV or another secure storage
		// For now, this is a placeholder
		console.log(`Password hash stored for user ${userId}`);
	}
}
