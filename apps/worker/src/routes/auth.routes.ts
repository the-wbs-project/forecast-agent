import { Hono } from 'hono';
import { Variables } from '../config';
import { Http } from '../services';

const auth = new Hono<{ E: Env; Variables: Variables }>();

auth
  .post('/login', Http.auth.loginAsync)
  .post('/register', Http.auth.registerAsync)
  .post('/refresh-token', Http.auth.refreshTokenAsync)
  .post('/logout', Http.auth.logoutAsync)
  .post('/change-password', Http.auth.changePasswordAsync)
  .post('/reset-password', Http.auth.resetPasswordAsync)
  .post('/confirm-reset-password', Http.auth.confirmResetPasswordAsync)
  .get('/me', Http.auth.getMeAsync);

export const AUTH_ROUTES = auth;