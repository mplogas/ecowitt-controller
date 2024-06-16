using Ecowitt.Controller.Model;
using FluentValidation;

namespace Ecowitt.Controller.Validator;

public class ApiDataValidator : AbstractValidator<ApiData>
{
    public ApiDataValidator()
    {
        RuleFor(x => x.PASSKEY).NotEmpty();
        
        //validator options
        // RuleFor(x => x.DeviceType).IsInEnum();
        // RuleFor(x => x.Data).NotEmpty();
        // RuleFor(x => x.Data).NotNull();
        // RuleFor(x => x.Data).Must(x => x.Count > 0).When(x => x.Data != null);
    }
}