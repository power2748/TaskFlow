//using TaskService.Handlers;
//using FluentValidation;

//namespace TaskService.Validators;

//public class UpdateTaskValidator : AbstractValidator<UpdateTaskCommand>
//{
//    public UpdateTaskValidator()
//    {
//        RuleFor(x => x.Title)
//            .NotEmpty().WithMessage("Название задачи обязательно")
//            .MaximumLength(200).WithMessage("Название не должно превышать 200 символов");

//        RuleFor(x => x.Description)
//            .MaximumLength(1000).WithMessage("Описание не должно превышать 1000 символов");
//    }
//}