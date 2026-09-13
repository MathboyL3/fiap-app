using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Oficina.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "clientes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    documento_tipo = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    documento_numero = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: false),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Telefone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clientes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "estoques",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PecaId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuantidadeDisponivel = table.Column<int>(type: "integer", nullable: false),
                    QuantidadeReservada = table.Column<int>(type: "integer", nullable: false),
                    LimiteMinimo = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_estoques", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ordens_servico",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClienteId = table.Column<Guid>(type: "uuid", nullable: false),
                    VeiculoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    valor_total_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    valor_total_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    CriadaEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IniciadaExecucaoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FinalizadaEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EntregueEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ObservacoesDiagnostico = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ordens_servico", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "pecas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Unidade = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    valor_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    valor_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pecas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "servicos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    valor_base_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    valor_base_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    TempoEstimado = table.Column<TimeSpan>(type: "interval", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_servicos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SenhaHash = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "veiculos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClienteId = table.Column<Guid>(type: "uuid", nullable: false),
                    placa = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    Marca = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Modelo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Ano = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_veiculos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "estoque_movimentacoes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EstoquePecaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Tipo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Quantidade = table.Column<int>(type: "integer", nullable: false),
                    OcorreuEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Referencia = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_estoque_movimentacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_estoque_movimentacoes_estoques_EstoquePecaId",
                        column: x => x.EstoquePecaId,
                        principalTable: "estoques",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "os_historico_status",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrdemDeServicoId = table.Column<Guid>(type: "uuid", nullable: false),
                    De = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Para = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    OcorreuEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Observacao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_os_historico_status", x => x.Id);
                    table.ForeignKey(
                        name: "FK_os_historico_status_ordens_servico_OrdemDeServicoId",
                        column: x => x.OrdemDeServicoId,
                        principalTable: "ordens_servico",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "os_itens_peca",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrdemDeServicoId = table.Column<Guid>(type: "uuid", nullable: false),
                    PecaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Descricao = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    valor_unitario_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    valor_unitario_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Quantidade = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_os_itens_peca", x => x.Id);
                    table.ForeignKey(
                        name: "FK_os_itens_peca_ordens_servico_OrdemDeServicoId",
                        column: x => x.OrdemDeServicoId,
                        principalTable: "ordens_servico",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "os_itens_servico",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrdemDeServicoId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServicoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Descricao = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    valor_unitario_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    valor_unitario_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Quantidade = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_os_itens_servico", x => x.Id);
                    table.ForeignKey(
                        name: "FK_os_itens_servico_ordens_servico_OrdemDeServicoId",
                        column: x => x.OrdemDeServicoId,
                        principalTable: "ordens_servico",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_clientes_documento_numero",
                table: "clientes",
                column: "documento_numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_estoque_movimentacoes_EstoquePecaId",
                table: "estoque_movimentacoes",
                column: "EstoquePecaId");

            migrationBuilder.CreateIndex(
                name: "IX_estoques_PecaId",
                table: "estoques",
                column: "PecaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ordens_servico_ClienteId",
                table: "ordens_servico",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_ordens_servico_Status",
                table: "ordens_servico",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_os_historico_status_OrdemDeServicoId",
                table: "os_historico_status",
                column: "OrdemDeServicoId");

            migrationBuilder.CreateIndex(
                name: "IX_os_itens_peca_OrdemDeServicoId",
                table: "os_itens_peca",
                column: "OrdemDeServicoId");

            migrationBuilder.CreateIndex(
                name: "IX_os_itens_servico_OrdemDeServicoId",
                table: "os_itens_servico",
                column: "OrdemDeServicoId");

            migrationBuilder.CreateIndex(
                name: "IX_pecas_Codigo",
                table: "pecas",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_Email",
                table: "usuarios",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_veiculos_ClienteId",
                table: "veiculos",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_veiculos_placa",
                table: "veiculos",
                column: "placa",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "clientes");

            migrationBuilder.DropTable(
                name: "estoque_movimentacoes");

            migrationBuilder.DropTable(
                name: "os_historico_status");

            migrationBuilder.DropTable(
                name: "os_itens_peca");

            migrationBuilder.DropTable(
                name: "os_itens_servico");

            migrationBuilder.DropTable(
                name: "pecas");

            migrationBuilder.DropTable(
                name: "servicos");

            migrationBuilder.DropTable(
                name: "usuarios");

            migrationBuilder.DropTable(
                name: "veiculos");

            migrationBuilder.DropTable(
                name: "estoques");

            migrationBuilder.DropTable(
                name: "ordens_servico");
        }
    }
}
