using UnityEngine;
using UnityEngine.UIElements;

public class UIToolkitView : MonoBehaviour
{
    private UIDocument _uiDocument;
    private UIToolkitViewModel _uiToolkitViewModel;

    private Button _damageButton;

    private void Awake()
    {
        _uiDocument = GetComponent<UIDocument>();
    }

    public void Bind(UIToolkitViewModel viewModel)
    {
        _uiToolkitViewModel = viewModel;
        
        var root = _uiDocument.rootVisualElement;
        
        root.dataSource = viewModel;
        
        _damageButton = root.Q<Button>("DamageButton");
        _damageButton.clicked += OnDamageButtonClicked;
    }

    private void OnDamageButtonClicked()
    {
        _uiToolkitViewModel.ExecuteTakeDamage(10);
    }

    private void OnDestroy()
    {
        _damageButton.clicked -= OnDamageButtonClicked;
    }
}
