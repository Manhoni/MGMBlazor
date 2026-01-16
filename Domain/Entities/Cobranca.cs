using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MGMBlazor.Domain.Entities;

[Table("Cobrancas")]
public class Cobranca
{
    [Key]
    public int Id { get; set; }

    // Relacionamento com a Nota Fiscal
    public int NotaFiscalEmitidaId { get; set; }
    
    [ForeignKey("NotaFiscalEmitidaId")]
    public virtual NotaFiscalEmitida NotaFiscalEmitida { get; set; } = null!;

    [Required]
    public long NossoNumero { get; set; }

    [Required]
    public string LinhaDigitavel { get; set; } = string.Empty;

    public string? CodigoBarras { get; set; }
    
    public string? QrCodePix { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Valor { get; set; }

    public DateTime DataVencimento { get; set; }

    [Required]
    public string Status { get; set; } = "Pendente"; // Pendente, Pago, Baixado

    public DateTime DataCadastro { get; set; } = DateTime.Now;
}