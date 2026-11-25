using GestaoPedidos.Application.DTO.Usuarios;
using GestaoPedidos.Application.Validators.Usuarios;
using GestaoPedidos.Domain.Abstractions.Usuarios;
using GestaoPedidos.Domain.Entities;
using GestaoPedidos.Domain.Enum;

namespace GestaoPedidos.Infrastructure.Security
{
    public class AdminBootstrapService : IHostedService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AdminBootstrapService> _logger;

        public AdminBootstrapService(
            IServiceScopeFactory scopeFactory,
            IConfiguration configuration,
            ILogger<AdminBootstrapService> logger)
        {
            _scopeFactory = scopeFactory;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var nome = _configuration["BootstrapAdmin:Nome"];
            var email = _configuration["BootstrapAdmin:Email"];
            var senha = _configuration["BootstrapAdmin:Senha"];

            if (string.IsNullOrWhiteSpace(nome)
                || string.IsNullOrWhiteSpace(email)
                || string.IsNullOrWhiteSpace(senha))
            {
                return;
            }

            var emailNormalizado = email.Trim().ToLowerInvariant();
            var validacao = new UsuarioCreateValidator().Validate(new UsuarioCreateDTO
            {
                Nome = nome,
                Email = emailNormalizado,
                Senha = senha
            });

            if (!validacao.IsValid)
            {
                throw new InvalidOperationException(
                    "Configuração inválida de BootstrapAdmin: " +
                    string.Join(" ", validacao.Errors.Select(erro => erro.ErrorMessage)));
            }

            await using var scope = _scopeFactory.CreateAsyncScope();
            var repository = scope.ServiceProvider.GetRequiredService<IUsuarioRepository>();
            var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            var existente = await repository.ObterPorEmail(emailNormalizado);

            if (existente is not null)
            {
                if (existente.Role != UserRole.Admin)
                {
                    throw new InvalidOperationException(
                        "O e-mail configurado em BootstrapAdmin já pertence a um usuário não administrador.");
                }

                return;
            }

            var admin = new Usuario(
                nome,
                emailNormalizado,
                passwordHasher.Hash(senha),
                UserRole.Admin);
            await repository.Cadastrar(admin);

            _logger.LogInformation(
                "Administrador inicial {AdminId} criado. Remova BootstrapAdmin:Senha da configuração.",
                admin.Id);
        }

        public Task StopAsync(CancellationToken cancellationToken)
            => Task.CompletedTask;
    }
}
