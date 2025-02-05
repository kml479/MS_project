﻿using CourseSales.Shared.Messages.Events;
using Mass = MassTransit;

namespace CourseSales.Services.Catalog.Services
{
    public interface ICourseService
    {
        Task<Response<NoContentResponse>> DeleteByIdAsync(string id);
        Task<Response<List<CourseResponseModel>>> GetAllAsync();
        Task<Response<List<CourseResponseModel>>> GetAllByUserIdAsync(string userId);
        Task<Response<CourseResponseModel>> GetByIdAsync(string id);
        Task<Response<CourseResponseModel>> InsertAsync(AddCourseRequstModel addCourseRequstModel);
        Task<Response<NoContentResponse>> UpdateAsync(UpdateCourseRequestModel updateCourseRequestModel);
    }

    public sealed class CourseManager : ICourseService
    {
        private readonly Mass.IPublishEndpoint _publishEndpoint;
        private readonly IMongoContext _mongoContext;
        private readonly IMapper _mapper;

        public CourseManager(
            IMapper mapper,
            IMongoContext mongoContext, 
            Mass.IPublishEndpoint publishEndpoint)
        {
            _mongoContext = mongoContext;
            _mapper = mapper;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<Response<List<CourseResponseModel>>> GetAllAsync()
        {
            var courses = await _mongoContext.Courses.Find(filter => true).ToListAsync();
            if (!courses?.Any() ?? false)
            {
                return Response<List<CourseResponseModel>>.Fail("Kurs bulunamadı.", HttpStatusCode.NotFound);
            }

            courses.ForEach(course =>
            {
                course.Category = _mongoContext.Categories.Find(filter => filter.Id.Equals(course.CategoryId)).SingleOrDefault();
            });

            var coursesResponseModel = _mapper.Map<List<CourseResponseModel>>(courses);
            return Response<List<CourseResponseModel>>.Success(coursesResponseModel, HttpStatusCode.OK);
        }

        public async Task<Response<CourseResponseModel>> GetByIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException(nameof(id));

            var course = await _mongoContext.Courses.Find(filter => filter.Id.Equals(id)).SingleOrDefaultAsync();
            if (course is null)
            {
                return Response<CourseResponseModel>.Fail("Kurs bulunamadı.", HttpStatusCode.NotFound);
            }

            course.Category = await _mongoContext.Categories.Find(filter => filter.Id.Equals(course.CategoryId)).SingleOrDefaultAsync();

            var courseResponseModel = _mapper.Map<CourseResponseModel>(course);
            return Response<CourseResponseModel>.Success(courseResponseModel, HttpStatusCode.OK);
        }

        public async Task<Response<List<CourseResponseModel>>> GetAllByUserIdAsync(string userId)
        {
            var courses = await _mongoContext.Courses.Find(filter => filter.UserId.Equals(userId)).ToListAsync();
            if (!courses?.Any() ?? false)
            {
                return Response<List<CourseResponseModel>>.Fail("Kurs bulunamadı.", HttpStatusCode.NotFound);
            }

            foreach (var course in courses)
            {
                course.Category = await _mongoContext.Categories.Find(filter => filter.Id.Equals(course.CategoryId)).SingleOrDefaultAsync();
            }

            var coursesResponseModel = _mapper.Map<List<CourseResponseModel>>(courses);
            return Response<List<CourseResponseModel>>.Success(coursesResponseModel, HttpStatusCode.OK);
        }

        public async Task<Response<CourseResponseModel>> InsertAsync(AddCourseRequstModel addCourseRequstModel)
        {
            var course = _mapper.Map<Course>(addCourseRequstModel);
            await _mongoContext.Courses.InsertOneAsync(course);

            var courseResponseModel = _mapper.Map<CourseResponseModel>(course);
            return Response<CourseResponseModel>.Success(courseResponseModel, HttpStatusCode.OK);
        }

        public async Task<Response<NoContentResponse>> UpdateAsync(UpdateCourseRequestModel updateCourseRequestModel)
        {
            var course = _mapper.Map<Course>(updateCourseRequestModel);
            course = await _mongoContext.Courses.FindOneAndReplaceAsync(p => p.Id.Equals(updateCourseRequestModel.Id), course);
            if (course is null)
            {
                return Response<NoContentResponse>.Fail("Kurs bulunamadı.", HttpStatusCode.NotFound);
            }

            CourseNameChangedEvent courseNameChangedEvent = new()
            {
                CourseId = course.Id,
                UpdatedName = updateCourseRequestModel.Name
            };

            await _publishEndpoint.Publish<CourseNameChangedEvent>(courseNameChangedEvent);

            return Response<NoContentResponse>.Success(HttpStatusCode.OK);
        }

        public async Task<Response<NoContentResponse>> DeleteByIdAsync(string id)
        {
            var deleteResult = await _mongoContext.Courses.DeleteOneAsync(filter => filter.Id.Equals(id));
            if (0 >= deleteResult.DeletedCount)
            {
                return Response<NoContentResponse>.Fail("Kurs bulunamadı.", HttpStatusCode.NotFound);
            }

            return Response<NoContentResponse>.Success(HttpStatusCode.OK);
        }
    }
}
