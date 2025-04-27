using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class WindowManager : MonoBehaviour
{
    public static WindowManager Layout;

    [SerializeField] private GameObject[] windows;


    private void Awake()
    {
        Layout = this;
        foreach(GameObject window in windows)
        {
            window.SetActive(false);
        }
    }

    public void OpenLayout(string name)
    {
        foreach(GameObject window in windows)
        {
            if (window.name == name) window.SetActive(true);
            else window.SetActive(false);
        }
    }

    private void Start()
    {
        OpenLayout("Loading");
    }



}
