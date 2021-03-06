package triggerstandalonebuild.common;

public interface iPluginConstants {
    String PLUGIN_TYPE = "triggerstandalonebuild";

    String PROPERTYKEY_EMAIL = "tsa.email";
    String PROPERTYKEY_PASSWORD = "tsa.password";
    String PROPERTYKEY_SRCPATH = "tsa.src";
    String PROPERTYKEY_PLATFORM_ANDROID = "tsa.platform.and";
    String PROPERTYKEY_PLATFORM_IOS = "tsa.platform.ios";
    String PROPERTYKEY_CONFIGKEYS = "tsa.configkeys";

    String PROPERTYKEY_ANDROIDKEYSTOREPATH = "tsa.andkeystore";
    String PROPERTYKEY_ANDROIDKEYSTOREPASSWORD = "tsa.andkeystorepass";
    String PROPERTYKEY_ANDROIDKEYALIAS = "tsa.andkeyalias";
    String PROPERTYKEY_ANDROIDKEYPASSWORD = "tsa.andkeypass";
    String PROPERTYKEY_ANDROIDIGNOREPATH = "tsa.andignore";
    String PROPERTYKEY_ANDROIDPACKAGENAME = "tsa.andpackage";

    String PROPERTYKEY_IOSCERTIFICATEPATH = "tsa.ioscert";
    String PROPERTYKEY_IOSCERTIFICATEPASSWORD = "tsa.ioscertpass";
    String PROPERTYKEY_IOSPROFILEPATH = "tsa.iosprofile";
    String PROPERTYKEY_IOSIGNOREPATH = "tsa.iosignore";
    String PROPERTYKEY_IOSPACKAGENAME = "tsa.iospackage";

    String TRIGGER_BUILD_URL = "https://trigger.io/standalone/package";
}
