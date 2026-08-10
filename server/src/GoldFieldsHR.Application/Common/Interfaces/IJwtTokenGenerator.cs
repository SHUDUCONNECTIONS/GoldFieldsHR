using GoldFieldsHR.Domain.Entities;

namespace GoldFieldsHR.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    (string Token, DateTime ExpiresAtUtc) GenerateToken(Guid userId, string email, Employee employee);
}
