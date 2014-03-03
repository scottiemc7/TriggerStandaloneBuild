<%@ taglib prefix="props" tagdir="/WEB-INF/tags/props" %>
<%@ taglib prefix="l" tagdir="/WEB-INF/tags/layout" %>
<%@ taglib prefix="c" uri="http://java.sun.com/jsp/jstl/core" %>
<%@ taglib prefix="forms" tagdir="/WEB-INF/tags/forms" %>
<jsp:useBean id="propertiesBean" scope="request" type="jetbrains.buildServer.controllers.BasePropertiesBean"/>

<tr>
    <th>
        <label for="tsa.email">Email: </label> <l:star/>
    </th>
    <td>
        <props:textProperty name="tsa.email" size="40" />
        <span class="error" id="error_tsa.email"></span>
        <span class="smallNote">The email address you used to sign up to Trigger.io.</span>
    </td>
</tr>

<tr>
    <th>
        <label for="tsa.password">Password: </label> <l:star/>
    </th>
    <td>
        <props:passwordProperty name="tsa.password" size="40" />
        <span class="error" id="error_tsa.password"></span>
        <span class="smallNote">The password you gave when you signed up for Trigger.io.</span>
    </td>
</tr>

<tr>
    <th>
        <label for="tsa.src">Src path: </label> <l:star/>
    </th>
    <td>
        <props:textProperty name="tsa.src" size="75" />
        <span class="error" id="error_tsa.src"></span>
        <span class="smallNote">The path to your src folder, relative to the checkout directory. The <b>config.json</b> file should be at the root of this folder.</span>
    </td>
</tr>

<tr>
    <th>
        <label>Build for platform: </label> <l:star/>
    </th>
    <td>
        <label for="tsa.platform.and" style="padding-right:3em"><props:checkboxProperty name="tsa.platform.and"/>Android</label>
        <label for="tsa.platform.ios"><props:checkboxProperty name="tsa.platform.ios" />iOS</label>
        <span class="error" id="error_tsa.platform"></span>
        <span class="smallNote">Select at least one platform</span>
    </td>
</tr>

<tr>
    <th>
        <label for="tsa.andkeystore">Android keystore path: </label>
    </th>
    <td>
        <props:textProperty name="tsa.andkeystore" size="75" />
        <span class="error" id="error_tsa.andkeystore"></span>
        <span class="smallNote">The path to the file created by the <b>keytool</b> utility.</span>
    </td>
</tr>

<tr>
    <th>
        <label for="tsa.andkeystorepass">Android keystore password: </label>
    </th>
    <td>
        <props:passwordProperty name="tsa.andkeystorepass" size="40" />
        <span class="error" id="error_tsa.andkeystorepass"></span>
        <span class="smallNote">The password needed to unlock your keystore.</span>
    </td>
</tr>

<tr>
    <th>
        <label for="tsa.andkeyalias">Android key alias: </label>
    </th>
    <td>
        <props:textProperty name="tsa.andkeyalias" size="40" />
        <span class="error" id="error_tsa.andkeyalias"></span>
        <span class="smallNote">The name of the key in your keystore.</span>
    </td>
</tr>

<tr>
    <th>
        <label for="tsa.andkeypass">Android key password: </label>
    </th>
    <td>
        <props:passwordProperty name="tsa.andkeypass" size="40" />
        <span class="error" id="error_tsa.andkeypass"></span>
        <span class="smallNote">The password for the individual key in your keystore.</span>
    </td>
</tr>

<tr>
    <th>
        <label for="tsa.andignore">Android ignore paths: </label>
    </th>
    <td>
        <props:textProperty name="tsa.andignore" size="75" />
        <span class="error" id="error_tsa.andignore"></span>
        <span class="smallNote">Paths to directories to exclude from Android builds, relative to the src directory. Multiple paths can be separated by a semi-colon.</span>
    </td>
</tr>

<tr>
    <th>
        <label for="tsa.andpackage">Android package name: </label>
    </th>
    <td>
        <props:textProperty name="tsa.andpackage" size="75" />
        <span class="error" id="error_tsa.andpackage"></span>
        <span class="smallNote">Name for the package without extension</span>
    </td>
</tr>

<tr>
    <th>
        <label for="tsa.ioscert">iOS certficate path: </label>
    </th>
    <td>
        <props:textProperty name="tsa.ioscert" size="75" />
        <span class="error" id="error_tsa.ioscert"></span>
        <span class="smallNote">A path to a .p12 file containing your exported iOS developer certificate.</span>
    </td>
</tr>

<tr>
    <th>
        <label for="tsa.ioscertpass">iOS certificate password: </label>
    </th>
    <td>
        <props:passwordProperty name="tsa.ioscertpass" size="40" />
        <span class="error" id="error_tsa.ioscertpass"></span>
        <span class="smallNote">Password you used when exporting your iOS developer certificate.</span>
    </td>
</tr>

<tr>
    <th>
        <label for="tsa.iosprofile">iOS provisioning profile: </label>
    </th>
    <td>
        <props:textProperty name="tsa.iosprofile" size="75" />
        <span class="error" id="error_tsa.iosprofile"></span>
        <span class="smallNote">A path to the .mobileprovision file you have downloaded from the iOS provisioning portal.</span>
    </td>
</tr>

<tr>
    <th>
        <label for="tsa.iosignore">iOS ignore paths: </label>
    </th>
    <td>
        <props:textProperty name="tsa.iosignore" size="75" />
        <span class="error" id="error_tsa.iosignore"></span>
        <span class="smallNote">Paths to directories to exclude from iOS builds, relative to the src directory. Multiple paths can be separated by a semi-colon.</span>
    </td>
</tr>

<tr>
    <th>
        <label for="tsa.iospackage">iOS package name: </label>
    </th>
    <td>
        <props:textProperty name="tsa.iospackage" size="75" />
        <span class="error" id="error_tsa.iospackage"></span>
        <span class="smallNote">Name for the package without extension</span>
    </td>
</tr>

<hr>

   <tr>
       <th>
           <label for="tsa.configkeys">Configuration key/value pairs: </label>
       </th>
       <td>
           <props:textProperty name="tsa.configkeys" size="75" />
           <span class="error" id="error_tsa.configkeys"></span>
           <span class="smallNote">Key/value pairs to be replaced in config.json. Key/values should be separated by a comma. Multiple key/value pairs should be separate by a semi-colon.</span>
           <span class="smallNote">Example: key1,value1;full\path\to\key2,value2</span>
       </td>
   </tr>
