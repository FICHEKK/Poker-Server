using UnityEngine;

namespace Dao
{
    public class DaoProviderInitializer : MonoBehaviour
    {
        private void Awake()
        {
            var databasePath = Application.persistentDataPath + "/Databases/clients.csv";
            DaoProvider.Dao = new FileDao(databasePath);
        }
    }
}
