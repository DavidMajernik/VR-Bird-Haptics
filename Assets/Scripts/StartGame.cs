using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartGame : MonoBehaviour
{
    [SerializeField] Button startButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
 
    public void LoadStart()
    {
               SceneManager.LoadScene("Demo");
    }



}


