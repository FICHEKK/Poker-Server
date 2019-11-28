using System;
using System.IO;
using Dao;
using UnityEngine;

namespace Test {
    public class Test : MonoBehaviour {
        public void Start() {
            bool didRegister = DaoProvider.Dao.Register("FIC", "1234");
            Debug.Log(didRegister);

            didRegister = DaoProvider.Dao.Register("Marko", "Nemec");
            Debug.Log(didRegister);

            DaoProvider.Dao.SetChipCount("Marko", 123);
        }
    }
    
}