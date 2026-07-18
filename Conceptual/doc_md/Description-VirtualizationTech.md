# Virtualization Technologies

---

## Terraform

Terraform is an open-source **Infrastructure-as-Code (IaC)** tool by HashiCorp. It allows users to define, provision, and manage infrastructure resources declaratively using **HCL (HashiCorp Configuration Language)** or JSON.

### Key Features

| Feature | Description |
|---------|-------------|
| **Declarative Language** | HCL — easy to understand and manage infrastructure as code |
| **Multi-Cloud Support** | AWS, Azure, GCP, on-premises, and more |
| **Resource Management** | VMs, networks, storage, etc. |
| **State Management** | Tracks current state of infrastructure |
| **Modularity** | Reusable modules for complex infrastructure |

### Main Components

| Component | Description |
|-----------|-------------|
| **Providers** | Define the cloud/infrastructure platform |
| **Resources** | Infrastructure components to manage (VMs, networks) |
| **Data Sources** | Fetch information from external/existing infrastructure |
| **Variables** | Parameterize configuration |
| **Outputs** | Expose values for use elsewhere in configuration |

### Key Terms

- **Terraform Module** — Reusable, self-contained code defining a set of infrastructure resources.
- **Terraform State File** — Tracks all infrastructure deployed by Terraform.

---

## Docker

Docker is an open-source **containerization platform** for building, deploying, and running applications. It decouples the application from the underlying infrastructure.

### What is Containerization?

Packaging an application with its dependencies (libraries, binaries) into a single unit called a **container**. Containers:
- Are isolated from each other.
- Run on any platform supporting container technology.
- Enable consistent development and deployment environments.

### Benefits of Containerization

| Benefit | Description |
|---------|-------------|
| **Efficiency** | Multiple containers on a single host; efficient resource use |
| **Portability** | Move between hosts easily; quick scale up/down |
| **Security** | Containers are isolated — harder for malware to spread |

---

## Docker vs Virtual Machines

| Feature | Virtual Machines | Docker Containers |
|---------|-----------------|-------------------|
| Resource usage | More | Less |
| Process isolation | Hardware level | OS level |
| Operating System | Separate OS per VM | Shared OS resources |
| Customization | Highly customizable | Easy custom setup |
| Creation time | Slow | Very quick |
| Boot time | Minutes | Seconds |

---

## Docker Concepts

### Docker File → Docker Image → Docker Container

```
DOCKER FILE  →  DOCKER IMAGE  →  DOCKER CONTAINER
```

- **Dockerfile** — Text file with commands to build an image.
- **Docker Image** — Executable package (template) from which containers are created.
- **Docker Container** — Running instance of a Docker image.

### Docker Components

| Component | Description |
|-----------|-------------|
| **Docker Client** | Sends build/pull/run commands to Docker Host |
| **Docker Host** | Contains the daemon, containers, and images |
| **Registry** | Stores Docker images (Docker Hub = public registry) |

### Docker Container Lifecycle

```
Create → Run → Pause → Unpause → Start → Stop → Restart → Kill → Destroy
```

### Common Docker Commands

| Command | Action |
|---------|--------|
| `docker build` | Build an image from a Dockerfile |
| `docker pull` | Download an image |
| `docker images` | List downloaded images |
| `docker stats` | Display container info |
| `docker kill` | Kill a running container |
| `docker info` | Display system-wide Docker info |

### Container Restart Policies

| Policy | Behaviour |
|--------|-----------|
| `Off` | Never restarts |
| `On-failure` | Restarts only on non-user failures |
| `Unless-stopped` | Restarts unless manually stopped |
| `Always` | Always restarts |

---

## Container Orchestration

Managing and coordinating containers in a distributed system (deployment, scaling, networking).

| Tool | Best For |
|------|----------|
| **Kubernetes** | Large-scale deployments |
| **Docker Compose** | Smaller deployments, already using Docker |
| **Mesos** | Flexible, highly scalable deployments |

---

## Hypervisor

A hypervisor is software that allows creation and management of **virtual machines (VMs)**. Also called a **Virtual Machine Monitor (VMM)**.

A single host computer can run multiple guest VMs by dividing resources (memory, CPU).

### Types of Hypervisor

#### Type 1 — Native / Bare Metal
Runs directly on physical hardware. Common in data centers.

**Pros:**
- Direct access to hardware — very efficient.
- More secure (no third-party OS layer).

**Cons:**
- Requires dedicated machine for management.

#### Type 2 — Hosted
Runs as an application on top of a host OS (e.g., VMware Player, Parallels Desktop). Common on PCs/endpoints.

**Pros:**
- Easy access to guest OS alongside host machine.
- Additional tools for host-guest coordination.

**Cons:**
- No direct hardware access — less efficient than Type 1.
- Host OS vulnerabilities affect guest VMs.

### Hypervisor Pros

| Advantage | Description |
|-----------|-------------|
| **Quickness** | Immediate VM creation without provisioning bare-metal servers |
| **Efficient** | Multiple VMs on a single host |
| **Portable** | VMs independent of physical machine |
| **Server Consolidation** | Central management dashboard |
| **Adaptable** | OS/apps not tied to specific hardware |
| **Data Replication** | Fast clone/replication operations |

### Hypervisor Cons

- Requires external administration interface.
- Less secure — expanded attack surface via host OS.
- Less dependable — host OS issues affect all VMs.
- Reduced access to host resources (shared with host OS).
- Cannot run VMs on personal PCs without additional solutions.

---

## Virtualization vs Containerization

| Aspect | Virtualization | Containerization |
|--------|---------------|-----------------|
| Abstraction Level | Physical machine | Application |
| OS | Each VM has its own OS | Share host OS kernel |
| Resource Use | Higher | Lower |
| Boot Time | Minutes | Seconds |
| Use Case | Full OS isolation | Lightweight app deployment |
