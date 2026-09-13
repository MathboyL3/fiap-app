namespace Oficina.Application.Common;

public class AppException : Exception
{
    public AppException(string message) : base(message) { }
}

public class NotFoundException : AppException
{
    public NotFoundException(string entidade, object id) : base($"{entidade} não encontrado(a). ID: {id}.") { }
}

public class ConflictException : AppException
{
    public ConflictException(string message) : base(message) { }
}
