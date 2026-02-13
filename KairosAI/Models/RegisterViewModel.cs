namespace KairosAI.Models
{
    public class RegisterViewModel
    {
        public string? FullName { get; set; }

        public string? Email { get; set; }

        public string? Password { get; set; }

        // Mantenemos esta propiedad para validarla manualmente en el controlador
        public string? ConfirmPassword { get; set; }

        // Checkbox de "Acepto riesgos de inversión"
        public bool TermsAccepted { get; set; }

    }
}
