using NieuwsAPI.Model;

namespace NieuwsAPI.Data
{
    public class NieuwsberichtRepository
    {
        private Dictionary<int, Nieuwsbericht> berichten;
        private int idTeller;

        public NieuwsberichtRepository()
        {
            berichten = new Dictionary<int, Nieuwsbericht>();
            idTeller = 0;
        }

        public IEnumerable<Nieuwsbericht> Messages
        {
            get { return berichten.Values; }
        }

        public Nieuwsbericht this[int id]
        {
            get { return berichten[id]; }
        }

        internal Nieuwsbericht Add(Nieuwsbericht message)
        {
            idTeller++;
            message.Id = idTeller;
            berichten[idTeller] = message;
            return message;
        }

        internal void Update(Nieuwsbericht message)
        {
            berichten[(int)message.Id] = message;
        }

        internal bool IdExists(int id)
        {
            return berichten.ContainsKey(id);
        }

        internal void Delete(int id)
        {
            berichten.Remove(id);
        }
    }
}
