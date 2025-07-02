export interface ApiResponse<T = unknown> {
	success: boolean;
	data?: T;
	message?: string;
	errors?: string[];
}

export interface PagedResult<T> {
	items: T[];
	totalCount: number;
	pageNumber: number;
	pageSize: number;
	totalPages: number;
}

export interface KVMetadata {
	id: string;
	createdAt: string;
	updatedAt: string;
	createdBy?: string;
	tags?: string[];
}

export interface PaginationQuery {
	pageNumber?: number;
	pageSize?: number;
}

export interface SearchQuery extends PaginationQuery {
	search?: string;
}
