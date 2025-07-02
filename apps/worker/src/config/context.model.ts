import type { Context as C } from "hono";
import type { Variables } from "./variables.model";
//
//  This is so we can pass a context which doesn't include request info.
//
export type ContextLocal = {
	env: Env;
	var: Variables;
	executionCtx: LocalExecutionContext;
};

export type Context = C<{ E: Env; Variables: Variables }>;

export interface LocalExecutionContext {
	waitUntil(promise: Promise<unknown>): void;
}
