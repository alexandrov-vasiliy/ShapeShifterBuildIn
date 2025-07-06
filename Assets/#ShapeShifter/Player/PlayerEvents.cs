namespace _ShapeShifter.Player
{
    public static class PlayerEvents
    {
        public static event System.Action OnPlayerEat;

        public static void RaisePlayerEat()
        {
            OnPlayerEat?.Invoke();
        }
    }
}