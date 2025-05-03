using AutoMapper;
using e_learning.DTOs;
using e_learning.Models;

namespace e_learning.Mappings
{
    public class QuizProfile : AutoMapper.Profile
    {
        public QuizProfile()
        {
            // تأكيد أننا نستخدم Profile من AutoMapper
            CreateMap<Quiz, QuizDto>()
                .ForMember(dest => dest.Duration,
                    opt => opt.MapFrom(src => src.TimeLimitMinutes));

            CreateMap<CreateQuizDto, Quiz>();

            CreateMap<Quiz, QuizDetailsDto>();

            CreateMap<Question, QuestionForStudentDto>();

            CreateMap<Answer, ChoiceForStudentDto>();

            CreateMap<QuizResult, QuizResultDto>()
                .ForMember(dest => dest.QuizTitle,
                    opt => opt.MapFrom(src => src.Quiz.Title));

            CreateMap<UserAnswer, UserAnswerDto>();
        }
    }
}