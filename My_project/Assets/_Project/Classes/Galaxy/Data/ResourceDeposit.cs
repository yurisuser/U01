namespace _Project.Scripts.Galaxy.Data
{
    public struct ResourceDeposit
    {
        public int DepositId; //id месторождения
        public int ResourceId; // id ресурса
        public float ResourcePurity; //Содержание ресурса (0-1) Чистота руды типа
        public float Availability; //Доступность ресурса для добычи. (0-1)
    }
}