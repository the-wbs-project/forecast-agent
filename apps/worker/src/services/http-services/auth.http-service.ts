import { Context } from '../../config';
import {
    LoginRequest,
    RegisterRequest,
    RefreshTokenRequest,
    ChangePasswordRequest,
    ResetPasswordRequest,
    ResetPasswordConfirmRequest,
    ApiResponse
} from '../../dto';

export class AuthHttpService {
    async loginAsync(ctx: Context): Promise<Response> {
        try {
            const request = await ctx.req.json<LoginRequest>();

            if (!request.email || !request.password) {
                return ctx.json({
                    success: false,
                    message: 'Email and password are required'
                }, 400);
            }

            const result = await ctx.var.authService.login(request);

            return ctx.json<ApiResponse<typeof result>>({
                success: true,
                data: result
            });
        } catch (error) {
            return ctx.json({
                success: false,
                message: error instanceof Error ? error.message : 'Login failed'
            }, 401);
        }
    }

    async registerAsync(ctx: Context): Promise<Response> {
        try {
            const request = await ctx.req.json<RegisterRequest>();

            if (!request.email || !request.password || !request.firstName || !request.lastName) {
                return ctx.json({
                    success: false,
                    message: 'Email, password, first name, and last name are required'
                }, 400);
            }

            const user = await ctx.var.authService.register(request);

            return ctx.json<ApiResponse<typeof user>>({
                success: true,
                data: user
            }, 201);
        } catch (error) {
            return ctx.json({
                success: false,
                message: error instanceof Error ? error.message : 'Registration failed'
            }, 400);
        }
    }

    async refreshTokenAsync(ctx: Context): Promise<Response> {
        try {
            const request = await ctx.req.json<RefreshTokenRequest>();

            if (!request.refreshToken) {
                return ctx.json({
                    success: false,
                    message: 'Refresh token is required'
                }, 400);
            }

            const result = await ctx.var.authService.refreshToken(request);

            return ctx.json<ApiResponse<typeof result>>({
                success: true,
                data: result
            });
        } catch (error) {
            return ctx.json({
                success: false,
                message: error instanceof Error ? error.message : 'Token refresh failed'
            }, 401);
        }
    }

    async logoutAsync(ctx: Context): Promise<Response> {
        try {
            const { refreshToken } = await ctx.req.json<{ refreshToken: string }>();

            if (!refreshToken) {
                return ctx.json({
                    success: false,
                    message: 'Refresh token is required'
                }, 400);
            }

            await ctx.var.authService.logout(refreshToken);

            return ctx.json({
                success: true,
                message: 'Logged out successfully'
            });
        } catch (error) {
            return ctx.json({
                success: false,
                message: error instanceof Error ? error.message : 'Logout failed'
            }, 400);
        }
    }

    async changePasswordAsync(ctx: Context): Promise<Response> {
        try {
            // Get user ID from JWT token
            const token = ctx.req.header('Authorization')?.replace('Bearer ', '');
            if (!token) {
                return ctx.json({
                    success: false,
                    message: 'Authentication required'
                }, 401);
            }

            const tokenPayload = await ctx.var.authService.validateToken(token);
            if (!tokenPayload) {
                return ctx.json({
                    success: false,
                    message: 'Invalid token'
                }, 401);
            }

            const request = await ctx.req.json<ChangePasswordRequest>();

            if (!request.currentPassword || !request.newPassword) {
                return ctx.json({
                    success: false,
                    message: 'Current password and new password are required'
                }, 400);
            }

            await ctx.var.authService.changePassword(tokenPayload.userId, request);

            return ctx.json({
                success: true,
                message: 'Password changed successfully'
            });
        } catch (error) {
            return ctx.json({
                success: false,
                message: error instanceof Error ? error.message : 'Password change failed'
            }, 400);
        }
    }

    async confirmResetPasswordAsync(ctx: Context): Promise<Response> {
        try {
            const request = await ctx.req.json<ResetPasswordConfirmRequest>();

            if (!request.token || !request.newPassword) {
                return ctx.json({
                    success: false,
                    message: 'Reset token and new password are required'
                }, 400);
            }

            await ctx.var.authService.confirmResetPassword(request);

            return ctx.json({
                success: true,
                message: 'Password reset successfully'
            });
        } catch (error) {
            return ctx.json({
                success: false,
                message: error instanceof Error ? error.message : 'Password reset confirmation failed'
            }, 400);
        }
    }

    async resetPasswordAsync(ctx: Context): Promise<Response> {
        try {
            const request = await ctx.req.json<ResetPasswordRequest>();

            if (!request.email) {
                return ctx.json({
                    success: false,
                    message: 'Email is required'
                }, 400);
            }

            await ctx.var.authService.resetPassword(request);

            return ctx.json({
                success: true,
                message: 'Password reset instructions sent to email'
            });
        } catch (error) {
            return ctx.json({
                success: false,
                message: error instanceof Error ? error.message : 'Password reset failed'
            }, 400);
        }
    }

    async getMeAsync(ctx: Context): Promise<Response> {
        try {
            const token = ctx.req.header('Authorization')?.replace('Bearer ', '');
            if (!token) {
                return ctx.json({
                    success: false,
                    message: 'Authentication required'
                }, 401);
            }

            const tokenPayload = await ctx.var.authService.validateToken(token);
            if (!tokenPayload) {
                return ctx.json({
                    success: false,
                    message: 'Invalid token'
                }, 401);
            }

            const user = await ctx.var.authService.getUserById(tokenPayload.userId);
            if (!user) {
                return ctx.json({
                    success: false,
                    message: 'User not found'
                }, 404);
            }

            return ctx.json<ApiResponse<typeof user>>({
                success: true,
                data: user
            });
        } catch (error) {
            return ctx.json({
                success: false,
                message: error instanceof Error ? error.message : 'Failed to get user'
            }, 500);
        }
    }

}