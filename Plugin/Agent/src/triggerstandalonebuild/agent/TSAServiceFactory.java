package triggerstandalonebuild.agent;

import jetbrains.buildServer.agent.AgentBuildRunnerInfo;
import jetbrains.buildServer.agent.artifacts.ArtifactsWatcher;
import jetbrains.buildServer.agent.runner.CommandLineBuildServiceFactory;
import triggerstandalonebuild.common.iPluginConstants;

public class TSAServiceFactory implements CommandLineBuildServiceFactory {
    private final ArtifactsWatcher artifactsWatcher;

    public TSAServiceFactory(ArtifactsWatcher artifactsWatcher) {
        this.artifactsWatcher = artifactsWatcher;
    }

    //@NotNull
    public jetbrains.buildServer.agent.runner.CommandLineBuildService createService() {
        return new TSAService(artifactsWatcher);
    }

    //@NotNull
    public jetbrains.buildServer.agent.AgentBuildRunnerInfo getBuildRunnerInfo() {
        return new AgentBuildRunnerInfo() {
            //@NotNull
            public String getType() {
                return iPluginConstants.PLUGIN_TYPE;
            }

            public boolean canRun(jetbrains.buildServer.agent.BuildAgentConfiguration agentConfiguration) {
                return agentConfiguration.getSystemInfo().isWindows();
            }
        };
    }
}
