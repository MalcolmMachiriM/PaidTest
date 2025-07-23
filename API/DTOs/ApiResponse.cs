﻿namespace API.DTOs
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
        public List<string>? Errors { get; set; }

        public static ApiResponse<T> SuccessResult(T data, string? message = null)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Data = data,
                Message = message
            };
        }

        public static ApiResponse<T> ErrorResult(string message, List<string>? errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Errors = errors
            };
        }


    }

    public class ApiResponse : ApiResponse<object>
    {
        public static ApiResponse Success(string? message = null)
        {
            return new ApiResponse
            {
                //Success = true,
                Message = message
            };
        }

        public static ApiResponse Error(string message, List<string>? errors = null)
        {
            return new ApiResponse
            {
                //Success = false,
                Message = message,
                Errors = errors
            };
        }
    }
}
