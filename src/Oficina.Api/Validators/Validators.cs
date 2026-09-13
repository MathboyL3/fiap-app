using FluentValidation;
using Oficina.Application.DTOs;

namespace Oficina.Api.Validators;

public class CriarClienteValidator : AbstractValidator<CriarClienteRequest>
{
    public CriarClienteValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Documento).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Telefone).NotEmpty().MaximumLength(30);
    }
}

public class CadastrarVeiculoValidator : AbstractValidator<CadastrarVeiculoRequest>
{
    public CadastrarVeiculoValidator()
    {
        RuleFor(x => x.ClienteId).NotEmpty();
        RuleFor(x => x.Placa).NotEmpty();
        RuleFor(x => x.Marca).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Modelo).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Ano).GreaterThan(1900).LessThanOrEqualTo(DateTime.UtcNow.Year + 1);
    }
}

public class CadastrarServicoValidator : AbstractValidator<CadastrarServicoRequest>
{
    public CadastrarServicoValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ValorBase).GreaterThanOrEqualTo(0);
        RuleFor(x => x.TempoEstimadoMinutos).GreaterThan(0);
    }
}

public class CadastrarPecaValidator : AbstractValidator<CadastrarPecaRequest>
{
    public CadastrarPecaValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Codigo).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Unidade).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Valor).GreaterThanOrEqualTo(0);
        RuleFor(x => x.QuantidadeInicial).GreaterThanOrEqualTo(0);
        RuleFor(x => x.LimiteMinimo).GreaterThanOrEqualTo(0);
    }
}

public class AbrirOSValidator : AbstractValidator<AbrirOSRequest>
{
    public AbrirOSValidator()
    {
        RuleFor(x => x.ClienteId).NotEmpty();
        RuleFor(x => x.VeiculoId).NotEmpty();
    }
}

public class IncluirItemServicoValidator : AbstractValidator<IncluirItemServicoRequest>
{
    public IncluirItemServicoValidator()
    {
        RuleFor(x => x.ServicoId).NotEmpty();
        RuleFor(x => x.Quantidade).GreaterThan(0);
    }
}

public class IncluirItemPecaValidator : AbstractValidator<IncluirItemPecaRequest>
{
    public IncluirItemPecaValidator()
    {
        RuleFor(x => x.PecaId).NotEmpty();
        RuleFor(x => x.Quantidade).GreaterThan(0);
    }
}

public class LoginValidator : AbstractValidator<LoginRequest>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Senha).NotEmpty().MinimumLength(8);
    }
}

public class CriarUsuarioValidator : AbstractValidator<CriarUsuarioRequest>
{
    public CriarUsuarioValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Senha).NotEmpty().MinimumLength(8);
        RuleFor(x => x.Role).NotEmpty();
    }
}
