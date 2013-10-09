package triggerstandalonebuild.server;

import jetbrains.buildServer.serverSide.InvalidProperty;
import jetbrains.buildServer.serverSide.PropertiesProcessor;
import jetbrains.buildServer.serverSide.RunType;
import jetbrains.buildServer.serverSide.RunTypeRegistry;
import triggerstandalonebuild.common.iPluginConstants;

import java.util.*;

public class TSABuildType extends RunType {

    public TSABuildType(final RunTypeRegistry registry) {
        registry.registerRunType(this);
    }

    @Override
    public String getType() {
        return iPluginConstants.PLUGIN_TYPE;
    }

    @Override
    public String getDisplayName() {
        return "Trigger.io Standalone Build";
    }

    @Override
    public String getDescription() {
        return "Runner for using the Trigger.io Standalone Build API";
    }

    @Override
    public PropertiesProcessor getRunnerPropertiesProcessor() {
        return new PropertiesProcessor() {
            public Collection<InvalidProperty> process(Map<String, String> properties) {
                ArrayList<InvalidProperty> err = new ArrayList<InvalidProperty>();

                //check email
                if(!keyExistsAndHasValue(iPluginConstants.PROPERTYKEY_EMAIL, properties))
                    err.add(0, new InvalidProperty(iPluginConstants.PROPERTYKEY_EMAIL, "Email required"));

                //check password
                if(!keyExistsAndHasValue(iPluginConstants.PROPERTYKEY_PASSWORD, properties))
                    err.add(0, new InvalidProperty(iPluginConstants.PROPERTYKEY_PASSWORD, "Password required"));

                //check src path
                if(!keyExistsAndHasValue(iPluginConstants.PROPERTYKEY_SRCPATH, properties))
                    err.add(0, new InvalidProperty(iPluginConstants.PROPERTYKEY_SRCPATH, "Invalid src path"));

                //check at least one platform selected
                boolean androidSelected = keyExistsAndHasValueEqualTo(iPluginConstants.PROPERTYKEY_PLATFORM_ANDROID,
                        "true", properties),
                        iOSSelected = keyExistsAndHasValueEqualTo(iPluginConstants.PROPERTYKEY_PLATFORM_IOS,
                            "true", properties);
                if(!androidSelected && !iOSSelected)
                    err.add(0, new InvalidProperty("tsa.platform", "You must select at least one platform"));

                //check android
                if(androidSelected) {
                    //check keystore path
                    if(!keyExistsAndHasValue(iPluginConstants.PROPERTYKEY_ANDROIDKEYSTOREPATH, properties))
                        err.add(0, new InvalidProperty(iPluginConstants.PROPERTYKEY_ANDROIDKEYSTOREPATH, "Invalid keystore path"));
                    //check keystore password
                    if(!keyExistsAndHasValue(iPluginConstants.PROPERTYKEY_ANDROIDKEYSTOREPASSWORD, properties))
                        err.add(0, new InvalidProperty(iPluginConstants.PROPERTYKEY_ANDROIDKEYSTOREPASSWORD, "Keystore password required"));
                    //check key alias
                    if(!keyExistsAndHasValue(iPluginConstants.PROPERTYKEY_ANDROIDKEYALIAS, properties))
                        err.add(0, new InvalidProperty(iPluginConstants.PROPERTYKEY_ANDROIDKEYALIAS, "Key alias required"));
                    //check key password
                    if(!keyExistsAndHasValue(iPluginConstants.PROPERTYKEY_ANDROIDKEYPASSWORD, properties))
                        err.add(0, new InvalidProperty(iPluginConstants.PROPERTYKEY_ANDROIDKEYPASSWORD, "Key password required"));
                }//end if

                if(iOSSelected) {
                    //check certificate path
                    if(!keyExistsAndHasValue(iPluginConstants.PROPERTYKEY_IOSCERTIFICATEPATH, properties))
                        err.add(0, new InvalidProperty(iPluginConstants.PROPERTYKEY_IOSCERTIFICATEPATH, "Invalid certificate path"));
                    //check certificate password
                    if(!keyExistsAndHasValue(iPluginConstants.PROPERTYKEY_IOSCERTIFICATEPASSWORD, properties))
                        err.add(0, new InvalidProperty(iPluginConstants.PROPERTYKEY_IOSCERTIFICATEPASSWORD, "Certificate password required"));
                    //check certificate path
                    if(!keyExistsAndHasValue(iPluginConstants.PROPERTYKEY_IOSPROFILEPATH, properties))
                        err.add(0, new InvalidProperty(iPluginConstants.PROPERTYKEY_IOSPROFILEPATH, "Invalid profile path"));
                }//end if

                return err;
            }

            private boolean keyExistsAndHasValueEqualTo(String key, String value, Map<String, String> properties) {
                if(properties.containsKey(key)) {
                    String val = properties.get(key);
                    return (val == null && value == null) || (val != null && val.compareTo(value) == 0);
                }
                else
                    return false;
            }

            private boolean keyExistsAndHasValue(String key, Map<String, String> properties) {
                return  properties.containsKey(key) && !isEmptyOrNull(properties.get(key));
            }

            private boolean isEmptyOrNull(String val) {
                return val == null || val.length() == 0;
            }
        };
    }

    @Override
    public String getEditRunnerParamsJspFilePath() {
        return "tsaEdit.jsp";
    }

    @Override
    public String getViewRunnerParamsJspFilePath() {
        return "tsaView.jsp";
    }

    @Override
    public Map<String, String> getDefaultRunnerProperties() {
        return null;
    }
}
