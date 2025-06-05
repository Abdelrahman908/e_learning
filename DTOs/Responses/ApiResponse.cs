namespace e_learning.DTOs.Responses
{
    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<string>? Errors { get; set; }
        public int? StatusCode { get; set; }

        public ApiResponse(bool success, string message, int? statusCode = null)
        {
            Success = success;
            Message = message;
            StatusCode = statusCode;
        }

        public static ApiResponse Ok(string message = "تمت العملية بنجاح") =>
            new ApiResponse(true, message, 200);

        public static ApiResponse Fail(string message, int statusCode = 400) =>
            new ApiResponse(false, message, statusCode);

        public static ApiResponse Fail(string message, List<string>? errors, int statusCode = 400) =>
            new ApiResponse(false, message, statusCode)
            {
                Errors = errors
            };

        public static ApiResponse NotFound(string message = "العنصر غير موجود") =>
            new ApiResponse(false, message, 404);

        // ✅ تم إضافة هذا الميثود لتصحيح خطأ CS0117 في الكود
        public static ApiResponse Error(string message, int statusCode = 400) =>
            new ApiResponse(false, message, statusCode);
    }

    public class ApiResponse<T> : ApiResponse
    {
        public T? Data { get; set; }

        public ApiResponse(bool success, string message, T? data, int? statusCode = null)
            : base(success, message, statusCode)
        {
            Data = data;
        }

        public static ApiResponse<T> SuccessResponse(T data, string message = "تمت العملية بنجاح") =>
            new ApiResponse<T>(true, message, data, 200);

        public static ApiResponse<T> SuccessResponse(T data, string message, int statusCode) =>
            new ApiResponse<T>(true, message, data, statusCode);

        public static ApiResponse<T> Error(string message, int statusCode = 400) =>
            new ApiResponse<T>(false, message, default, statusCode);

        public static ApiResponse<T> Error(string message, List<string>? errors, int statusCode = 400) =>
            new ApiResponse<T>(false, message, default, statusCode)
            {
                Errors = errors
            };

        public static ApiResponse<T> NotFound(string message = "العنصر غير موجود") =>
            new ApiResponse<T>(false, message, default, 404);
    }
}
