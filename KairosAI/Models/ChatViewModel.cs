using System;
using System.Collections.Generic;

namespace KairosAI.Models.ViewModels
{
    public class ChatViewModel
    {
        // La lista de mensajes que se mostrará en la pantalla
        public List<ChatMessage> ConversationHistory { get; set; } = new();

        // El campo donde el usuario escribe su nuevo mensaje
        public string CurrentInput { get; set; }
    }

    public class ChatMessage
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Role { get; set; } // "User" o "Kairos"
        public string Text { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;

        // Propiedad visual: ¿Es un mensaje de alerta/riesgo?
        public bool IsWarning { get; set; }
    }
}