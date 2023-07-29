using System;
using System.Collections;
using System.IO;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Main : MonoBehaviour
{
    private ToggleButton _teacherBtn;
    private ToggleButton _studentBtn;
    private ToggleButton _teacherStudentBtn;
    private ToggleButton _suddenNoiseBtn;
    private ToggleButton _mediaBtn;

    private Image _teacherImg;
    private Image _studentImg;
    private Image _teacherStudentImg;
    private Image _suddenNoiseImg;
    private Image _mediaImg;

    private TextMeshProUGUI _debugText;
    private TextMeshProUGUI _timerText;

    private Button _startBtn;
    private TextMeshProUGUI _startBtnText;
    private bool _recording;
    private string _name;
    private Coroutine _samplingRoutine;
    private float _samplingRate;
    private DateTime _startTime;

    private TMP_InputField _samplingRateInput;

    private int _teacher;
    private int _student;
    private int _teacherStudent;
    private int _suddenNoise;
    private int _media;

    private int _countDown;
    private double _nextMark;

    void Start()
    {
        GameObject teacherGO = GameObject.Find("TeacherBtn");
        GameObject studentGO = GameObject.Find("StudentBtn");
        GameObject teacherStudentGO = GameObject.Find("TeacherStudentBtn");
        GameObject suddenNoiseGO = GameObject.Find("SuddenNoiseBtn");
        GameObject mediaGO = GameObject.Find("MediaBtn");

        _teacherBtn = teacherGO.GetComponent<ToggleButton>();
        _studentBtn = studentGO.GetComponent<ToggleButton>();
        _teacherStudentBtn = teacherStudentGO.GetComponent<ToggleButton>();
        _suddenNoiseBtn = suddenNoiseGO.GetComponent<ToggleButton>();
        _mediaBtn = mediaGO.GetComponent<ToggleButton>();

        _teacherImg = teacherGO.GetComponent<Image>();
        _studentImg = studentGO.GetComponent<Image>();
        _teacherStudentImg = teacherStudentGO.GetComponent<Image>();
        _suddenNoiseImg = suddenNoiseGO.GetComponent<Image>();
        _mediaImg = mediaGO.GetComponent<Image>();

        GameObject startGO = GameObject.Find("StartBtn");
        _startBtn = startGO.GetComponent<Button>();
        _startBtnText = startGO.GetComponentInChildren<TextMeshProUGUI>();

        _startBtn.onClick.AddListener(StartStop);

        UpdateStartBtnLabel();

        _samplingRateInput = GameObject.Find("SamplingRateInput").GetComponent<TMP_InputField>();

        _debugText = GameObject.Find("Debug").GetComponent<TextMeshProUGUI>();

        _timerText = GameObject.Find("Timer").GetComponent<TextMeshProUGUI>();

        UpdateBackgroundColor(false);
    }

    void StartStop()
    {
        _recording = !_recording;
        UpdateStartBtnLabel();
        _samplingRateInput.enabled = !_recording;
        StopAllCoroutines();
        if (_recording)
        {
            ClearFlags();
            _samplingRate = 5;
            float.TryParse(_samplingRateInput.text, out _samplingRate);
            if (float.IsNaN(_samplingRate) || _samplingRate < 0.1f)
                _samplingRate = 0.1f;
            _samplingRateInput.text = _samplingRate.ToString();
            _countDown = 3;
            _nextMark = _samplingRate;
            StartCoroutine(Delay(1));
        }
        else
        {
            UpdateBackgroundColor(false);
            _debugText.text = "";
        }
    }

    private void UpdateStartBtnLabel()
    {
        _startBtnText.text = _recording ? "Stop" : "Start";
    }

    void Update()
    {
        _teacherImg.color = Color.white;
        _studentImg.color = Color.white;
        _teacherStudentImg.color = Color.white;
        _suddenNoiseImg.color = Color.white;
        _mediaImg.color = Color.white;

        if (Input.GetKey("5") || _mediaBtn.pressed)
        {
            _media++;
            _mediaImg.color = new Color(24f / 255f, 231f / 255f, 97f / 255f);
        }
        else
        {
            if (Input.GetKey("4") || _suddenNoiseBtn.pressed)
            {
                _suddenNoise++;
                _suddenNoiseImg.color = new Color(250f / 255f, 220f / 255f, 57f / 255f);
            }
            else
            {
                if (Input.GetKey("3") || _teacherStudentBtn.pressed)
                {
                    _teacherStudent++;
                    _teacherStudentImg.color = new Color(229f / 255f, 24f / 255f, 231f / 255f);
                }
                else
                {
                    if (Input.GetKey("1") || _teacherBtn.pressed)
                    {
                        _teacher++;
                        _teacherImg.color = new Color(239f / 255f, 89f / 255f, 156f / 255f);
                    }

                    if (Input.GetKey("2") || _studentBtn.pressed)
                    {
                        _student++;
                        _studentImg.color = new Color(29f / 255f, 171f / 255f, 226f / 255f);
                    }
                }
            }
        }

        if (_recording)
        {
            if(_countDown > 0)
                _timerText.text = "Starting in: " + _countDown;
            else
            {
                TimeSpan timeSpan = DateTime.Now - _startTime;
                _timerText.text = "Timer: " + timeSpan.ToString(@"hh\:mm\:ss\:fff");
                if (timeSpan.TotalMilliseconds > (_nextMark * 1000))
                {
                    _nextMark += _samplingRate;
                    try
                    {
                        int category = 4;
                        if (_media > 0)
                            category = 6;
                        else if (_suddenNoise > 0)
                            category = 5;
                        else if (_teacherStudent > 0)
                            category = 3;
                        else if (_teacher > 0 || _student > 0)
                        {
                            if (_teacher == 0)
                                category = 2;
                            else if (_student == 0)
                                category = 1;
                            else
                                category = _teacher > _student ? 1 : 2;
                        }

                        File.AppendAllText(_name + ".xls", category + Environment.NewLine);
                        UpdateBackgroundColor(true);
                        _debugText.text = "Last Recorded Value: " + category;
                    }
                    catch (Exception ex)
                    {
                        UpdateBackgroundColor(false);
                        _debugText.text = "Error: " + ex.Message;
                    }
                    finally
                    {
                        ClearFlags();
                    }
                }
            }
        }
        else
        {
            _timerText.text = "";
        }
    }

    IEnumerator Delay(float time)
    {
        yield return new WaitForSeconds(time);
        StopAllCoroutines();
        if (_countDown >= 0)
        {
            _countDown--;
            if (_countDown == 0)
            {
                _startTime = DateTime.Now;
                _name = _startTime.ToString("yyyy-MM-dd HH-mm-ss-fff");
            }
            else
                StartCoroutine(Delay(1));
        }
    }

    private void ClearFlags()
    {
        _teacher = 0;
        _student = 0;
        _teacherStudent = 0;
        _suddenNoise = 0;
        _media = 0;
    }

    private void UpdateBackgroundColor(bool recording)
    {
        Camera.main.backgroundColor = recording ? new Color(120f / 255f, 48f / 255f, 129f / 255f) : new Color(74f / 255f, 48f / 255f, 130f / 255f);
    }
}
