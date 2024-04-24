using FluentValidation;

public class SimpleImmutableOptionValidator : AbstractValidator<SimpleImmutableOption>
{
    public SimpleImmutableOptionValidator()
    {
        RuleFor(x => x.DbConnectionString).NotEmpty();
    }
}

