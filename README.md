# CUIT Service

It will return information based on a provided CUIT number.

## Continuous Deployment to test and production environments

We are following the same criteria than [Doppler
Forms](https://github.com/MakingSense/doppler-forms/blob/master/README.md#continuous-deployment-to-test-and-production-environments).

## To do

- [x] EditorConfig, Prettier, etc
- [x] Continuous Deployment (Jenkins, DockerHub)
- [x] Define resources (URLs, schema, etc)
- [x] Expose hardcoded information (it could be also useful in test environments)
- [ ] Validate JWT token
- [ ] Rate limit for normal users, unlimited for Doppler SU
- [ ] Connect with our backend service
- [ ] Deploy to our Swarm
- [ ] Keep our secrets and configurations in encrypted files here or in our
      Swarm repository
