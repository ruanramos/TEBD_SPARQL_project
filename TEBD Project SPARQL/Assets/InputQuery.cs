using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InputQuery : MonoBehaviour
{
    InputField inputfield;

    private void Awake()
    {
        inputfield = GetComponent<InputField>();

    }

    IEnumerator FieldFix()
    {
        print("Shift + Enter newline.");
        inputfield.ActivateInputField();

        yield return null;

        inputfield.text += "\n";
        inputfield.MoveTextEnd(false);
    }
    void Update()
    {

        if (EventSystem.current.currentSelectedGameObject == inputfield.gameObject)
        {
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyUp(KeyCode.Return))
            {
                StartCoroutine(FieldFix());
            }
        }
    }
}
