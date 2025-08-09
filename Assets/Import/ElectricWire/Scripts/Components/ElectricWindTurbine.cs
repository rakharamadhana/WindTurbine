using System;
using UnityEngine;
using TMPro;

namespace ElectricWire
{
    [Serializable]
    public class ElectricWindTurbineJsonData
    {
        public float delay;
        public ElectricWindTurbineJsonData(float newDelay) { delay = newDelay; }
    }

    public class ElectricWindTurbine : ElectricComponent, ISaveJsonData
    {
        [Header("Turbine Settings")]
        public Animator animator;
        public bool workWithoutWind = false;
        public GameObject energyGauge;
        public string windName = "WindZone";
        public float minWindForce = 0.2f;
        public float maxWindForce = 2f;

        [Header("UI References")]
        public TextMeshProUGUI powerText;
        public TextMeshProUGUI vinText;
        public TextMeshProUGUI v2Text;

        [Header("Scale Controller")]
        public UniformScaleController scaleController; // Drag & drop in Inspector

        private double totalPower = 0.0;
        private WindZone windGameObject;

        public static double currentPower = 0.0;

        private void Start()
        {
            if (scaleController == null)
            {
                GameObject uiManager = GameObject.Find("UIManager");
                if (uiManager != null)
                {
                    scaleController = uiManager.GetComponentInChildren<UniformScaleController>();
                    if (scaleController != null)
                    {
                        scaleController.targetObject = this.gameObject; // ✅ Assign itself
                    }
                }
            }

            AutoAssignReferencesIfNull();
            StartManagement();
        }


        private void AutoAssignReferencesIfNull()
        {
            if (scaleController == null)
                scaleController = FindObjectOfType<UniformScaleController>();

            if (powerText == null)
                powerText = GameObject.Find("PowerText")?.GetComponent<TextMeshProUGUI>();

            if (v2Text == null)
                v2Text = GameObject.Find("V2Text")?.GetComponent<TextMeshProUGUI>();        
        }



        public override void StartManagement()
        {
            StartEnergyManagement();
        }

        public string GetJsonData()
        {
            return JsonUtility.ToJson(new ElectricWindTurbineJsonData(0));
        }

        public void SetupFromJsonData(string jsonData)
        {
            var data = JsonUtility.FromJson<ElectricWindTurbineJsonData>(jsonData);
            if (data == null)
            {
                Debug.LogWarning("Missing turbine JSON data. Try saving again.");
                return;
            }
            StartManagement();
        }

        private void OnDisable()
        {
            CancelInvoke();
        }

        private void StartEnergyManagement()
        {
            CancelInvoke();
            InvokeRepeating(nameof(ManageEnergy), 1f, 1f);
        }

        // ✅ Use double precision to match Python
        private double CalculatePower(double v)
        {
            double k1 = 4.74422;
            double k2 = 0.0013629;
            double k3 = 0.847773;
            return k1 / (k2 + Math.Exp(-k3 * v));
        }

        private void ManageEnergy()
        {
            // ✅ Ensure we have a WindZone
            if (!workWithoutWind && windGameObject == null)
            {
                var found = GameObject.Find(windName);
                if (found != null)
                    windGameObject = found.GetComponent<WindZone>();
                else
                {
                    workWithoutWind = true;
                    Debug.LogWarning("No WindZone found. Turbine runs without wind.");
                }
            }

            // ✅ Double precision wind speed
            double windStrength = workWithoutWind ? 8.0 : windGameObject.windMain;
            double vin = Math.Round(windStrength, 2);
            double diameter = Math.Round(GetDiameter(), 2);

            // ✅ Effective wind speed formula: v2 = vin * (126 / diameter)^2
            double v2 = Math.Round(vin * Math.Pow(126.0 / diameter, 2.0), 2);

            // ✅ Calculate turbine power
            double turbinePower = 0.0;
            if (diameter >= 110.0 && diameter <= 140.0)
            {
                if (v2 > 0.0 && v2 < 12.0)
                    turbinePower = CalculatePower(v2);
                else if (v2 >= 12.0 && v2 <= 22.5)
                    turbinePower = 3450.0;
            }

            turbinePower = Math.Round(turbinePower, 2); // ✅ Match Python output
            currentPower = (float)turbinePower;

            // ✅ Update UI with 2 decimal places
            if (powerText != null) powerText.text = $"{turbinePower:0.00} kW";
            if (vinText != null) vinText.text = $"Vin: {vin:0.00} m/s";
            if (v2Text != null) v2Text.text = $"{v2:0.00} m/s";

            // Rotor speed multiplier based on effective wind speed
            float rotorSpeed = (float)v2 / 12f; // normalize: max speed at 12 m/s
            animator.speed = Mathf.Clamp(rotorSpeed, 0f, 3f); // limit max spin

            // ✅ Convert to float only for Unity components
            totalPower = Math.Min(turbinePower / 3450.0, 1.0);
            if (energyGauge != null)
                energyGauge.transform.localScale = new Vector3((float)totalPower, 1f, 1f);

            // ✅ Energy logic
            GetSetIsEnergized = turbinePower > 0.0;
            GetSetIsOn = IsEnergized();
            animator.SetBool("IsOn", GetSetIsOn);

            // Broadcast turbine power to all buildings
            foreach (BuildingLightSwitch building in FindObjectsOfType<BuildingLightSwitch>())
            {
                building.UpdateLight((float)turbinePower);
            }

            ActivateOutput();
        }

        private double GetDiameter()
        {
            double realScale = scaleController != null ? scaleController.realScale : 1.0;
            double diameter = 126.0 * realScale;
            return Math.Round(diameter, 2); // ✅ Return with 2 decimals
        }

        public override void ConnectWire(GameObject wire, bool isInput, int index)
        {
            base.ConnectWire(wire, isInput, index);
            ActivateOutput();
        }

        public override float IsDrainEnergy(int index)
        {
            return IsOn() ? drainEnergy : 0f;
        }

        public override void EnergizeByWire(bool onOff, int index) { }
    }
}
