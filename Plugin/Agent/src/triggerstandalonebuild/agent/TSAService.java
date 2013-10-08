package triggerstandalonebuild.agent;

import jetbrains.buildServer.RunBuildException;
import jetbrains.buildServer.agent.AgentRunningBuild;
import jetbrains.buildServer.agent.artifacts.ArtifactsWatcher;
import jetbrains.buildServer.agent.runner.CommandLineBuildService;
import jetbrains.buildServer.agent.runner.SimpleProgramCommandLine;
import jetbrains.buildServer.util.StringUtil;
import sun.misc.BASE64Encoder;
import triggerstandalonebuild.common.iPluginConstants;

import java.io.UnsupportedEncodingException;
import java.util.ArrayList;
import java.util.List;
import java.util.Map;

public class TSAService extends CommandLineBuildService {
    private final ArtifactsWatcher artifactsWatcher;

    public TSAService(ArtifactsWatcher artifactsWatcher) {
        this.artifactsWatcher = artifactsWatcher;
    }

    @Override
    public jetbrains.buildServer.agent.runner.ProgramCommandLine makeProgramCommandLine() throws RunBuildException {
        AgentRunningBuild build = getBuild();

        List<String> args = createArgs();

        return new SimpleProgramCommandLine(build,
                build.getAgentConfiguration().getAgentPluginsDirectory() +  "\\" + iPluginConstants.PLUGIN_TYPE + "\\TriggerStandaloneConsole.exe",
                args);
    }

    private List<String> createArgs() {
        Map<String,String> parameters = getBuild().getRunnerParameters();
        List<String> result = new ArrayList<String>();

        result.add(parameters.get(iPluginConstants.PROPERTYKEY_EMAIL));
        result.add(parameters.get(iPluginConstants.PROPERTYKEY_PASSWORD));
        result.add(parameters.get(iPluginConstants.PROPERTYKEY_PLATFORM_ANDROID));
/*
        BASE64Encoder encoder = new BASE64Encoder();

        String program = null;
        try {
            program = encoder.encode(parameters.get(PluginConstants.PROPERTY_SCRIPT_CONTENT).getBytes("UTF8"));
        } catch (UnsupportedEncodingException e) {
            // are you sure you don't know UTF8?
        }

        result.add(program);

        result.add(PluginConstants.OUTPUT_PATH);
        result.add(PluginConstants.OUTPUT_FILE_NAME);

        String namespaces = parameters.get(PluginConstants.PROPERTY_SCRIPT_NAMESPACES);
        result.add(namespaces == null || StringUtil.isEmpty(namespaces) ? ";" : StringUtil.convertLineSeparators(namespaces, ";"));

        String references = parameters.get(PluginConstants.PROPERTY_SCRIPT_REFERENCES);
        result.add(references == null || StringUtil.isEmpty(references) ? ";" : StringUtil.convertLineSeparators(references, ";"));
*/
        return result;
    }
}

