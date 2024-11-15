using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.IO;

namespace Keyboard
{
    struct val_t
    {
        public long value;
        public bool correct;
    }

    struct ResultSubtest
    {
        public float time;
        public float correctRate;
    }

    struct ResultTest
    {
        public List<ResultSubtest> subtests;
        public float totalTime;
        public float averageTime;
        public float averageCorrectRate;
    }

    struct ResultMain
    {
        public List<ResultTest> tests;
        public float totalTime;
        public float averageTime;
        public float averageCorrectRate;
    }

    public class Manager : MonoBehaviour
    {
        [SerializeField] List<Test> tests;
        [SerializeField] GameObject mainMenu;
        [SerializeField] TMP_InputField nameInput;
        [Header("Controls")]
        [SerializeField] int count, testsToDo = 3;
        [SerializeField] bool autoStart = true;

        System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch(), subtimer = new System.Diagnostics.Stopwatch(), submaintimer = new System.Diagnostics.Stopwatch(), maintimer = new System.Diagnostics.Stopwatch();
        long pointer = 0, test = 1, mainTest = -1;
        string testName;
        float rangeStart = 1;
        val_t[] val;
        bool inputBlock = false;

        Test currentTest;
        ResultMain result;
        ResultTest currentResult;
        ResultSubtest currentSubresult;

        void Start()
        {
            val = new val_t[count];
        }

        public void StartMainTest()
        {
            mainTest = -1;
            mainMenu.SetActive(false);
            result = new();
            result.tests = new();
            testName = nameInput.text;
            maintimer.Reset(); maintimer.Start();
            currentTest = null;

            Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/" + testName);

            StartTest();
        }

        public void StartTest()
        {
            if (currentTest != null) currentTest.testMenu.SetActive(false);
            currentTest = tests[(int)++mainTest];
            tests[(int)mainTest].testMenu.SetActive(true);
            rangeStart = currentTest.includeZero ? 0 : 1;
            submaintimer.Reset(); submaintimer.Start();
            timer.Reset(); timer.Start();
            currentResult = new();
            currentResult.subtests = new();
            ResetTest();
        }

        public void RestartTest()
        {
            rangeStart = currentTest.includeZero ? 0 : 1;
            timer.Reset(); timer.Start();
            currentSubresult = new();
            ResetTest();
        }

        public void UpdateInput(int value)
        {
            if (pointer == count || inputBlock) return;
            else if (pointer == 0) for (int i = 0; i < count; i++) { GetTextIn(i).SetText("_"); GetTextIn(i).color = Color.white; }

            GetTextIn(pointer).SetText("*");
            val[pointer].value = value;
            val[pointer].correct = value.ToString() == GetTextEx(pointer).text;
            pointer = ++pointer >= count ? 0 : pointer;

            if (currentTest.autoEnter && pointer == 0) Enter();
        }

        public void ResetTest()
        {
            pointer = 0; currentTest.progressBar.fillAmount = 0;
            for (int i = 0; i < count; i++) 
            {
                val[i].value = 0;
                val[i].correct = false;
                GetTextEx(i).SetText(Mathf.RoundToInt(Random.Range(rangeStart, 9)).ToString());
                GetTextIn(i).SetText("_");
                GetTextIn(i).color = Color.white;
            }
            inputBlock = false;
        }

        public void Enter()
        {
            timer.Stop();
            pointer = count;
            inputBlock = true;

            int p = 0;
            for (int i = 0; i < count; i++)
            {
                GetTextIn(i).SetText(val[i].value.ToString());
                if (val[i].correct)
                {
                    GetTextIn(i).color = Color.green;
                    p++;
                } else GetTextIn(i).color = Color.red;
            }

            currentSubresult.correctRate = (p / (float)count) * 100f;

            StartCoroutine(End(++test > testsToDo));
        }

        public void Delete()
        {
            if (pointer > 0)
            {
                pointer--;
                GetTextIn(pointer).SetText("_");
                val[pointer].value = 0;
            }
        }

        private IEnumerator End(bool last)
        {
            subtimer.Reset(); subtimer.Start();

            while (subtimer.ElapsedMilliseconds <= 3000)
            {
                currentTest.progressBar.fillAmount = Mathf.Clamp01(subtimer.ElapsedMilliseconds / 3000f);
                yield return null;
            }

            subtimer.Stop();
            currentTest.progressBar.fillAmount = 0;

            timer.Stop();
            currentSubresult.time = timer.ElapsedMilliseconds / 1000f;
            currentResult.subtests.Add(currentSubresult);

            string json = JsonUtility.ToJson(currentSubresult);
            File.WriteAllText(Directory.GetCurrentDirectory() + "/" + testName + "/subtest-" + (result.tests.Count + 1) + "-" + currentResult.subtests.Count + ".json", json);

            if (mainTest >= (tests.Count - 1))
            {
                test = 1;
                submaintimer.Stop();
                currentResult.totalTime = submaintimer.ElapsedMilliseconds / 1000f;
                currentResult.averageCorrectRate = 0;
                currentResult.averageTime = 0;
                foreach (var res in currentResult.subtests)
                {
                    currentResult.averageCorrectRate += res.correctRate;
                    currentResult.averageTime += res.time;
                }
                currentResult.averageCorrectRate /= currentResult.subtests.Count;
                currentResult.averageTime /= currentResult.subtests.Count;
                result.tests.Add(currentResult);

                json = JsonUtility.ToJson(currentResult);
                File.WriteAllText(Directory.GetCurrentDirectory() + "/" + testName + "/test-" + result.tests.Count + ".json", json);

                maintimer.Stop();
                result.totalTime = maintimer.ElapsedMilliseconds / 1000f;
                result.averageCorrectRate = 0;
                result.averageTime = 0;
                foreach (var res in result.tests)
                {
                    result.averageCorrectRate += res.averageCorrectRate;
                    result.averageTime += res.averageTime;
                }
                result.averageCorrectRate /= result.tests.Count;
                result.averageTime /= result.tests.Count;
                tests[(int)mainTest].testMenu.SetActive(false);
                mainMenu.SetActive(true);

                json = JsonUtility.ToJson(result);
                File.WriteAllText(Directory.GetCurrentDirectory() + "/" + testName + "/main" + ".json", json);
            }
            else if (last)
            {
                test = 1;
                submaintimer.Stop();
                currentResult.totalTime = submaintimer.ElapsedMilliseconds / 1000f;
                currentResult.averageCorrectRate = 0;
                currentResult.averageTime = 0;
                foreach (var res in currentResult.subtests)
                {
                    currentResult.averageCorrectRate += res.correctRate;
                    currentResult.averageTime += res.time;
                }
                currentResult.averageCorrectRate /= currentResult.subtests.Count;
                currentResult.averageTime /= currentResult.subtests.Count;
                result.tests.Add(currentResult);

                json = JsonUtility.ToJson(currentResult);
                File.WriteAllText(Directory.GetCurrentDirectory() + "/" + testName + "/test-" + result.tests.Count + ".json", json);

                StartTest();
            }
            else RestartTest();
        }

        TMP_Text GetTextIn(long value)
        {
            switch (value)
            {
                case 0:
                    return currentTest.In1;
                case 1:
                    return currentTest.In2;
                case 2:
                    return currentTest.In3;
                default:
                    return currentTest.In4;
            }
        }
        TMP_Text GetTextEx(long value)
        {
            switch (value)
            {
                case 0:
                    return currentTest.Ex1;
                case 1:
                    return currentTest.Ex2;
                case 2:
                    return currentTest.Ex3;
                default:
                    return currentTest.Ex4;
            }
        }
    }
}
