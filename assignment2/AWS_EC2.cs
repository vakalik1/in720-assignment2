using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Amazon;
using Amazon.EC2;
using Amazon.EC2.Model;
namespace assignment2
{
    //* Was able to finish this assignment by the help of:
    //* Gowtham, Sakinah and Lewis 
    
    class AWS_EC2
    {
        //variables
        static AmazonEC2Client ec2Client;
        static String instanceId;
        static List<String> instanceIds;

        static void Main(String[] args)
        {
            ec2Client = new AmazonEC2Client(RegionEndpoint.USWest1);
            instanceId = getInstanceId();
            instanceIds = new List<String> { instanceId };

            if (instanceId == null)
            {
                createInstance();
            }


            // Feedback system - Tell the user the current state its Instance is in
            Console.WriteLine("The current instance's status is " + getInstanceStatus());
            Console.WriteLine("Enter a command:");
            String temp = Console.ReadLine();
            if (temp == "start")
            {
                if (getInstanceStatus() == "running")
                {
                    Console.WriteLine("The instance is already running.");
                }
                else
                {
                    startInstance();
                    Console.WriteLine("The current instance's status is " + getInstanceStatus());
                }
            }

            if (temp == "status")
            {
                Console.WriteLine("The current instance's status is " + getInstanceStatus());
            }

            if (temp == "stop")
            {
                if (getInstanceStatus() == "stopped")
                {
                    Console.WriteLine("The instance has already been stopped.");
                }
                else
                {
                    stopInstance();
                    Console.WriteLine("The current instance's status is " + getInstanceStatus());
                }
            }

            if (temp == "terminate")
            {
                if (getInstanceStatus() == "running")
                {
                    terminateInstance();
                    Console.WriteLine("The current instance's status is " + getInstanceStatus());
                }
                else
                {
                    Console.WriteLine("The instance has already been terminated.");
                }
            }

            Console.ReadLine();
        }

        //Creating Instances Method - new instance is created
        static void createInstance()
        {
            RunInstancesRequest create = new RunInstancesRequest("ami-31490d51", 1, 1);
            create.InstanceType = InstanceType.T2Nano;

            RunInstancesResponse run = ec2Client.RunInstances(create);

            string instanceId = run.Reservation.Instances[0].InstanceId;

            List<string> instanceList = new List<string>() { instanceId };
            List<Tag> tagList = new List<Tag>();
            tagList.Add(new Tag("Name", "Vakalala"));
            CreateTagsRequest tagRequest = new CreateTagsRequest(instanceList, tagList);

            ec2Client.CreateTags(tagRequest);
        }
        
        //Start Instance Method - starts the current instance
        static void startInstance()
        {
            StartInstancesRequest start = new StartInstancesRequest(instanceIds);
            ec2Client.StartInstances(start);
        }

        //Stop Instance Method - stops the current instance
        static void stopInstance()
        {
            StopInstancesRequest stop = new StopInstancesRequest(instanceIds);
            ec2Client.StopInstances(stop);
        }
        

        //Instance Status Method - gets the current status of the Instance
        static String getInstanceStatus()
        {
            DescribeInstanceStatusRequest state = new DescribeInstanceStatusRequest();
            state.IncludeAllInstances = true;
            state.InstanceIds = instanceIds;
            DescribeInstanceStatusResponse response = ec2Client.DescribeInstanceStatus(state);
            String status = response.InstanceStatuses[0].InstanceState.Name;
            return status;
        }


        //Terminate Instance Method - Terminates instance that has already been stopped by it instanceIDs
        static void terminateInstance() 
        {
            TerminateInstancesRequest terminate = new TerminateInstancesRequest(instanceIds);
            ec2Client.TerminateInstances(terminate);
        }

        //Instance Response Method - When an instance is started, this method provides feedback to the user on the state of the Instance
        static DescribeInstancesResponse getInstanceResponse()
        {
            DescribeInstancesRequest instanceRequest = new DescribeInstancesRequest();
            List<Filter> filters = new List<Filter>();
            List<String> tagFilter = new List<String>() { "Vakalala" };
            List<String> stateFilter = new List<String>() { "pending", "running", "shutdown", "stopping", "stopped"};
            filters.Add(new Filter("tag-value", tagFilter));
            filters.Add(new Filter("instance-state-name", stateFilter));
            instanceRequest.Filters = filters;

            DescribeInstancesResponse response = ec2Client.DescribeInstances(instanceRequest);
            return response;
        }

        //InstanceID Method - Grabs instancesID from AWS EC2
        static String getInstanceId()
        {
            String instanceId = null;
            DescribeInstancesResponse response = getInstanceResponse();

            if (response.Reservations.Count > 0)
            {
                instanceId = response.Reservations[0].Instances[0].InstanceId;
            }

            return instanceId;
        }
    }
}
