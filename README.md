# Overview
Bookstore is a full-featured web application built using ASP.NET Core MVC and Entity Framework. The application allows users to browse through a variety of books, add them to their cart, place orders, and manage their account. It also includes administrative features for managing products, categories, companies, and orders. The project incorporates user authentication and authorization, enabling different roles for customers and administrators. For payment, the app integrates with Stripe to handle secure transactions. The project explain the use of various modern technologies and best practices such as the Repository Pattern, dependency injection, and database seeding with migrations.

# Features

<h3> 1-User Authentication & Authorization:</h3><br>
-Register and login functionality for users.<br>
-Role-based authorization (Admin & Customer roles).<br>
-Identity framework integration to manage users.

<h3>2-CRUD Operations:</h3><br>
-Manage books (create, read, update, delete).<br>
-Manage book categories and companies.<br>
-Manage customer orders and view order history.<br>
-Manage shopping cart (add, remove items).

<h3>3-Payment Integration:</h3><br>
-Stripe payment gateway integration for secure payments.

<h3>4-Admin Panel:</h3><br>
-Admin users can add, edit, and delete books, categories, and companies.<br>
-Admin users can view and manage customer orders.

<h3>5-Search & Filtering:</h3><br>
-Users can search for books and filter them by category or company.

<h3>6-Responsive Design:</h3><br>
-Fully responsive and mobile-friendly design using Bootstrap.

# Technology Stack
ASP.NET Core MVC: For building the web application using the Model-View-Controller pattern.<br>
Entity Framework Core: For database management and interaction using code-first migrations.<br> 
SQL Server: As the database for storing books, users, orders, etc.<br>
ASP.NET Identity: For handling user registration, login, and role management.<br>
Stripe Payment Gateway: For secure payment processing.<br>
Razor Pages & Views: For dynamic content rendering.<br>
Dependency Injection: For managing services in a loosely coupled manner.<br>
Repository Pattern: For interacting with the database through a clean abstraction.<br>
Bootstrap: For responsive and mobile-friendly user interface.<br>
