using System;
using System.Collections.Generic;

namespace KairosAI.Models.ViewModels
{
    public class ChatViewModel
    {
        // La lista de mensajes que se mostrará en la pantalla
        public List<ChatMessage> ConversationHistory { get; set; } = new();

        // El campo donde el usuario escribe su nuevo mensaje
        public string CurrentInput { get; set; } = string.Empty;
    }

    public class ChatMessage
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        // "User" o "Kairos"
        public string Role { get; set; } = string.Empty;

        // La respuesta final pulida
        public string Text { get; set; } = string.Empty;

        // PROPIEDAD DE ESCALABILIDAD: 
        // Preparada para modelos de razonamiento (ej. DeepSeek o Phi-4). 
        // Con Llama 3.3 Instruct, se mantiene vacía y la vista la oculta automáticamente.
        public string Reasoning { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; } = DateTime.Now;

        // Propiedad visual: ¿Es un mensaje de alerta/riesgo?
        public bool IsWarning { get; set; }
    }
}