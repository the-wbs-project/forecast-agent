import { KVMetadata } from '../dto';

export interface KVService {
  get<T>(key: string): Promise<T | null>;
  put<T>(key: string, value: T, metadata?: KVMetadata): Promise<void>;
  delete(key: string): Promise<void>;
  list(prefix?: string, limit?: number, cursor?: string): Promise<KVListResult>;
}

export interface KVListResult {
  keys: Array<{
    name: string;
    metadata?: KVMetadata;
  }>;
  list_complete: boolean;
  cursor?: string;
}

export class CloudflareKVService implements KVService {
  constructor(private kv: KVNamespace) {}

  async get<T>(key: string): Promise<T | null> {
    const value = await this.kv.get(key, { type: 'json' });
    return value as T | null;
  }

  async put<T>(key: string, value: T, metadata?: KVMetadata): Promise<void> {
    const options: KVNamespacePutOptions = {};
    if (metadata) {
      options.metadata = metadata;
    }
    await this.kv.put(key, JSON.stringify(value), options);
  }

  async delete(key: string): Promise<void> {
    await this.kv.delete(key);
  }

  async list(prefix?: string, limit?: number, cursor?: string): Promise<KVListResult> {
    const options: KVNamespaceListOptions = {};
    if (prefix) options.prefix = prefix;
    if (limit) options.limit = limit;
    if (cursor) options.cursor = cursor;

    const result = await this.kv.list(options);
    return {
      keys: result.keys.map(key => ({
        name: key.name,
        metadata: key.metadata as KVMetadata | undefined
      })),
      list_complete: result.list_complete,
      cursor: 'cursor' in result ? result.cursor : undefined
    };
  }
}

export const createKVService = (kv: KVNamespace): KVService => {
  return new CloudflareKVService(kv);
};