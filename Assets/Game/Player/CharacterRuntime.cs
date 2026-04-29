namespace SteelRain.Player
{
    public sealed class CharacterRuntime
    {
        public CharacterDefinition Definition { get; }

        public CharacterRuntime(CharacterDefinition definition)
        {
            Definition = definition;
        }
    }
}
