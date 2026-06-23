//using Contracts.Tasks;
//using FluentValidation;

//namespace TaskService.Validators;

//public class CreateTaskValidator : AbstractValidator<CreateTask>
//{
//    public CreateTaskValidator()
//    {
//        Console.WriteLine("CreateTaskValidator CREATED");

//        RuleFor(x => x.Title)
//            .NotEmpty().WithMessage("Название задачи обязательно")
//            .MaximumLength(200).WithMessage("Название не должно превышать 200 символов")
//            .Must(x =>
//            {
//                Console.WriteLine($"Validate Title = '{x}'");
//                return !string.IsNullOrWhiteSpace(x);
//            });

//        RuleFor(x => x.Description)
//            .MaximumLength(1000).WithMessage("Описание не должно превышать 1000 символов");

//        RuleFor(x => x.CreatedBy)
//            .NotEmpty().WithMessage("ID пользователя обязателен");
//    }
//}
