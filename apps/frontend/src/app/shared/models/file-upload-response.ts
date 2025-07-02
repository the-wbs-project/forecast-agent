export interface FileUploadResponse {
	fileName: string;
	fileSize: number;
	uploadedAt: Date | string;
	url?: string;
}
