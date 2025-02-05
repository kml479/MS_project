﻿namespace CourseSales.Web.Services.Catalogs
{
    public sealed class CatalogService : ICatalogService
    {
        private readonly HttpClient _httpClient;
        private readonly IPhotoStockService _photoStockService;
        private readonly PhotoHelper _photoHelper;

        public CatalogService(
            HttpClient httpClient,
            IPhotoStockService photoStockService, 
            PhotoHelper photoHelper)
        {
            _httpClient = httpClient;
            _photoStockService = photoStockService;
            _photoHelper = photoHelper;
        }


        public async Task<bool> CreateCourseAsync(CourseCreateInput courseCreateInput)
        {
            var resultPhotoService = await _photoStockService.UploadPhotoAsync(courseCreateInput.PhotoFormFile);
            if (resultPhotoService is not null)
            {
                courseCreateInput.ImagePath = resultPhotoService.Url;
            }

            var httpResponseMessage = await _httpClient.PostAsJsonAsync("courses/add", courseCreateInput);
            return httpResponseMessage.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteCourseAsync(string courseId)
        {
            var httpResponseMessage = await _httpClient.DeleteAsync($"courses/delete/{courseId}");
            return httpResponseMessage.IsSuccessStatusCode;
        }

        public async Task<List<CategoryViewModel>> GetAllCategoryAsync()
        {
            var httpResponseMessage = await _httpClient.GetAsync("categories/getall");
            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                return null;
            }

            var responseCategoriesViewModel = await httpResponseMessage.Content.ReadFromJsonAsync<Response<List<CategoryViewModel>>>();
            return responseCategoriesViewModel.Data;
        }

        public async Task<List<CourseViewModel>> GetAllCourseAsync()
        {
            var httpResponseMessage = await _httpClient.GetAsync("courses/getall");
            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                return null;
            }

            var responseCoursesViewModel = await httpResponseMessage.Content.ReadFromJsonAsync<Response<List<CourseViewModel>>>();
            responseCoursesViewModel.Data.ForEach(x =>
            {
                x.ImagePath = _photoHelper.GetPhotoStockUrl(x.ImagePath);
            });

            return responseCoursesViewModel.Data;
        }

        public async Task<List<CourseViewModel>> GetAllCourseByUserIdAsync(string userId)
        {
            var httpResponseMessage = await _httpClient.GetAsync($"courses/getallbyuserid/{userId}");
            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                return null;
            }

            var responseCourseViewModel = await httpResponseMessage.Content.ReadFromJsonAsync<Response<List<CourseViewModel>>>();
            responseCourseViewModel.Data.ForEach(x =>
            {
                x.ImagePath = _photoHelper.GetPhotoStockUrl(x.ImagePath);
            });

            return responseCourseViewModel.Data;
        }

        public async Task<CourseViewModel> GetByCourseId(string courseId)
        {
            var httpResponseMessage = await _httpClient.GetAsync($"courses/getbyid/{courseId}");
            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                return null;
            }

            var responseCourseViewModel = await httpResponseMessage.Content.ReadFromJsonAsync<Response<CourseViewModel>>();
            responseCourseViewModel.Data.StockImagePath = _photoHelper.GetPhotoStockUrl(responseCourseViewModel.Data.ImagePath);

            return responseCourseViewModel.Data;
        }

        public async Task<bool> UpdateCourseAsync(CourseUpdateInput courseUpdateInput)
        {
            var resultPhotoService = await _photoStockService.UploadPhotoAsync(courseUpdateInput.PhotoFormFile);
            if (resultPhotoService is not null)
            {
                await _photoStockService.DeletePhotoAsync(courseUpdateInput.ImagePath);
                courseUpdateInput.ImagePath = resultPhotoService.Url;
            }
            var httpResponseMessage = await _httpClient.PutAsJsonAsync("courses/update", courseUpdateInput);
            return httpResponseMessage.IsSuccessStatusCode;
        }
    }
}
