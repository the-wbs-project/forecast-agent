export interface LoginRequest {
	email: string;
	password: string;
}

export interface LoginResponse {
	token: string;
	refreshToken: string;
	user: UserInfo;
	expiresIn: number;
}

export interface RegisterRequest {
	email: string;
	password: string;
	firstName: string;
	lastName: string;
	organizationId?: string;
}

export interface UserInfo {
	id: string;
	email: string;
	firstName: string;
	lastName: string;
	fullName: string;
	organizationId?: string;
	organizationName?: string;
	role: string;
	isActive: boolean;
}

export interface TokenPayload {
	userId: string;
	email: string;
	organizationId?: string;
	role: string;
	iat: number;
	exp: number;
}

export interface RefreshTokenRequest {
	refreshToken: string;
}

export interface ChangePasswordRequest {
	currentPassword: string;
	newPassword: string;
}

export interface ResetPasswordRequest {
	email: string;
}

export interface ResetPasswordConfirmRequest {
	token: string;
	newPassword: string;
}
