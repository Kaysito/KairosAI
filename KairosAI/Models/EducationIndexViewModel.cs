namespace KairosAI.Models
{
    public class EducationIndexViewModel
    {
        public int TotalPoints { get; set; } // Puntos actuales del usuario
        public List<CourseModule> Modules { get; set; } = new();
    }

    // Objeto auxiliar para cada tarjeta de curso
    public class CourseModule
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Level { get; set; } // "Básico", "Intermedio", "Avanzado"
        public int RewardPoints { get; set; } // Puntos que ganas al terminar (+50 XP)
        public int Progress { get; set; } // 0 a 100
        public bool IsLocked { get; set; } // True si necesita completar el anterior
        public string Icon { get; set; } // "Book", "Chart", "Shield" (Para usar con Lucide en el front)
    }

    // Vista de Detalle: El contenido de la lección
    public class LessonViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string ContentHtml { get; set; } // El texto educativo
        public string VideoUrl { get; set; } // Opcional: Link a YouTube
        public int RewardPoints { get; set; }
        public bool IsCompleted { get; set; }

        // Navegación
        public int? NextLessonId { get; set; }
        public int? PreviousLessonId { get; set; }
    }
}
