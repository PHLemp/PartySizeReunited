namespace PartySizeReunited.McMMenu.Options
{
    public class WarSailsOptions
    {
        public bool IsActivate { get; set; }
        public int BonusBoats { get; set; }
        // Garde-fou technique uniquement : la valeur par défaut ne doit jamais
        // limiter un groupe de taille réaliste, même avec de petites coques.
        public int MaximumIdealShips { get; set; } = 250;
        public bool OnlyApplyToPlayer { get; set; }
    }
}
