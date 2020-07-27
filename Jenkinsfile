pipeline {
    agent any
    stages {
        stage('Verify Format') {
            steps {
                sh 'docker build --target verify-format .'
            }
        }
        stage('Verify .sh files') {
            steps {
                sh 'docker build --target verify-sh .'
            }
        }
        stage('Restore') {
            steps {
                sh 'docker build --target restore .'
            }
        }
        stage('Build') {
            steps {
                sh 'docker build --target build .'
            }
        }
        stage('Test') {
            steps {
                sh '''docker build \
                --target test \
                .'''
            }
        }
        stage('Publish pre-release images from pull request') {
            when {
                changeRequest target: 'master'
            }
            steps {
                sh 'sh build-n-publish.sh --commit=${GIT_COMMIT} --name=pr-${CHANGE_ID}'
            }
        }
        stage('Publish pre-release images from master') {
            when {
                branch 'master'
            }
            steps {
                sh 'sh build-n-publish.sh --commit=${GIT_COMMIT} --name=master'
            }
        }
        stage('Publish pre-release images from INT') {
            when {
                branch 'INT'
            }
            steps {
                sh 'sh build-n-publish.sh --commit=${GIT_COMMIT} --name=INT'
            }
        }
        stage('Publish final version images') {
            when {
                expression {
                    return isVersionTag(readCurrentTag())
                }
            }
            steps {
                sh 'sh build-n-publish.sh --commit=${GIT_COMMIT} --version=${TAG_NAME}'
            }
        }
        stage('Generate version') {
            when {
                branch 'master'
            }
            steps {
                sh 'echo "TODO: generate a tag automatically"'
            }
        }
    }
}



def boolean isVersionTag(String tag) {
    echo "checking version tag $tag"

    if (tag == null) {
        return false
    }

    // use your preferred pattern
    def tagMatcher = tag =~ /v\d+\.\d+\.\d+/

    return tagMatcher.matches()
}

def CHANGE_ID = env.CHANGE_ID

// https://stackoverflow.com/questions/56030364/buildingtag-always-returns-false
// workaround https://issues.jenkins-ci.org/browse/JENKINS-55987
// TODO: read this value from Jenkins provided metadata
def String readCurrentTag() {
    return sh(returnStdout: true, script: "git describe --tags --match v?*.?*.?* --abbrev=0 --exact-match || echo ''").trim()
}
