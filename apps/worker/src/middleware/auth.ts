import type { Next, Context } from "hono";

export async function authenticateUser(ctx: Context, next: Next) {
    ctx.set('user', {
        organizationId: '123'
    });

    /*
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

    ctx.set('user', tokenPayload);
    */

    await next();
}