using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Keyboard
{
    
    public class GeneralManager : MonoBehaviour
    {
        [Header("Controls")]
        [SerializeField] int testsToDo = 3;

        System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();

        private void StartTest()
        {
            timer.Start();
        }
    }
}
