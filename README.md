# Ommel Samvirke - Coorporating local associations
OmmelSamvirke is a group of local associations in the town Ommel on the Danish island of Ærø. This repository contains the code for the website used by Ommel Samvirke ([ommelsamvirke.com](https://ommelsamvirke.com)).

## Contents
- [Introduction](#introduction)
- [Service Modules](#service-modules)
  - [Emails](#emails)
    - [Email Sending](#email-sending)
    - [Analytics & Reporting](#analytics--reporting)
    - [Contact Lists](#contact-lists)
    - [Email Validation](#email-validation)
    - [Templates](#templates)

## Introduction
The website is used for a wide array of tasks. The frontend for the system is a Blazor Server application that interacts with the system's service modules. The following service modules are currently implemented:
- Emails: The system is able to send emails to indivual recipients and contact lists through Azure Communication Services.

## Service Modules
The following sections contain detailed descriptions of the features offered by each service module. The following emojis are used to indicated if a feature has already been implemented or if it's an upcoming feautre. 

✅ Implemented feature <br>
⌛ Upcoming feature 

## Emails

## Email Module Requirements

### Email Sending
✅ **Send Emails to Recipients:**  
Users can send an email to one or more recipients, including one or more attachments.

✅ **Send Emails to Contact Lists:**  
Users can send emails (with or without attachments) to a contact list via batching. Emails are sent by default using the "To" field but can be configured to use BCC.

✅ **Service Limits Queries:**  
Users can query the current service limits (e.g., 5,000 emails per minute and 20,000 per hour).

⌛ **Sending Status Tracking:**  
Users can query for the sending status of an email sent by the system. (Running, failed, completed, etc.)

### Analytics & Reporting
✅ **Email Count Tracking:**  
Users can query the number of emails sent over specified time intervals.

✅ **Load vs. Service Limit Reporting:**  
Users can generate reports comparing email load against service limits. These reports include:
- A dataset showing the number of emails sent during a subinterval of the provided interval.
- A dataset showing the cumulative number of emails sent during the interval.

✅ **Daily Statistics:**  
A daily timer triggered job records:
- The number of subscribers per contact list.
- The total number of emails sent the previous day.
- The number of unique recipients emailed.

✅ **Custom Reporting:**  
Users can generate reports for specified time periods based on the daily statistics.

### Contact Lists
✅ **Contact List Creation**  
Users can create a contact list with zero or more contacts / email addresses.

✅ **Address Management:**  
- Admins can add or remove addresses from a contact list.
- Users can subscribe to or unsubscribe from a contact list.

✅ **Unsubscribe Process:**
When the system unsubscribes the user, it sends a confirmation email containing a link that can be used if the user wishes to undo the unsubscribe action. The link is valid for 14 days.

✅ **Unsubscribe Links:**  
All emails sent to a recipient via a contact list contains an unsubscribe link. 

✅ **Contact List Queries:**  
- Users can query a contact list to see its associated email addresses.
- Users can search for a specific email address to see which contact lists it's included in.

### Email Validation
✅ **Validation Checks:**  
The module can identify email addresses with an invalid structure within a given list.

### Templates
✅ **Template Support:**  
The system supports email templates with features such as partials, title extraction, and parameters. For development, a Blazor application can run locally to preview the email templates.
